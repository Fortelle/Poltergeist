using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations;
using Poltergeist.Operations.Background;
using Poltergeist.Operations.Foreground;
using Poltergeist.Operations.Macros;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.Emulators;

public class EmulatorModule : MacroModule
{
    public const string InputModeKey = "adb.inputmode";

    static EmulatorModule()
    {
        GlobalOptions.Add(new OptionDefinition<string>(AdbService.IpAddressKey)
        {
            DisplayLabel = "IP Address",
            Category = "ADB",
        });

        GlobalOptions.Add(new PathOption(AdbService.ExePathKey)
        {
            DisplayLabel = "Exe file",
            Category = "ADB",
        });

        GlobalOptions.Add(new OptionDefinition<bool>(CapturingProvider.PreviewCaptureKey)
        {
            DisplayLabel = "Preview captured image",
            Category = "Debug",
        });
    }

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.UserOptions.Add(new OptionDefinition<bool>(AdbService.KeepAliveKey, true)
        {
            DisplayLabel = "Keep adb server alive",
            Description = "Skips killing the adb server when the macro is completed. " +
                "This helps when you are planning to launch the macro frequently in a short time. " +
                "You can use the \"kill-server\" action to kill the adb server manually.",
            Category = "ADB",
        });

        macro.UserOptions.Add(new OptionDefinition<InputMode>(InputModeKey, InputMode.ADB)
        {
            DisplayLabel = "Input mode",
            Category = "ADB",
        });

        macro.Actions.Add(KillServerAction);
    }

    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<TimerService>();
        processor.Services.AddSingleton<AndroidEmulatorOperator>();
        processor.Services.AddSingleton<RandomEx>();
        processor.Services.AddSingleton<DistributionService>();

        var inputMode = processor.Options.Get<InputMode>(InputModeKey);

        switch (inputMode)
        {
            case InputMode.ADB:
                {
                    processor.Services.AddSingleton<TerminalService>();
                    processor.Services.AddSingleton<AdbService>();
                    processor.Services.AddSingleton<AdbInputService>();

                    processor.Services.AddSingleton<CapturingProvider, AdbCapturingService>();

                    processor.Services.AddSingleton<IEmulatorInputProvider, EmulatorAdbService>();
                }
                break;
            case InputMode.ADB_Background:
                {
                    processor.Services.AddSingleton<TerminalService>();
                    processor.Services.AddSingleton<AdbService>();
                    processor.Services.AddSingleton<AdbInputService>();

                    processor.Services.AddSingleton<BackgroundLocatingService>();
                    processor.Services.AddSingleton<ILocatingProvider>(x => x.GetRequiredService<BackgroundLocatingService>());
                    processor.Services.AddSingleton<CapturingProvider, BackgroundCapturingService>();

                    processor.Services.AddSingleton<IEmulatorInputProvider, EmulatorAdbService>();
                }
                break;
            case InputMode.Mouse:
                {
                    processor.Services.AddSingleton<ForegroundLocatingService>();
                    processor.Services.AddSingleton<ILocatingProvider>(x => x.GetRequiredService<ForegroundLocatingService>());
                    processor.Services.AddSingleton<ForegroundMouseService>();

                    processor.Services.AddSingleton<CapturingProvider, ForegroundCapturingService>();
                    processor.Services.AddSingleton<IEmulatorInputProvider, EmulatorMouseService>();
                }
                break;
        }
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        var repeats = processor.GetService<LoopService>();

        repeats.Before += (e) =>
        {
            var inputMode = e.Processor.Options.Get<InputMode>(InputModeKey);

            if (inputMode is InputMode.ADB or InputMode.ADB_Background)
            {
                var adb = e.Processor.GetService<AdbService>();
                if (!adb.Connect())
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (inputMode is InputMode.ADB_Background or InputMode.Mouse)
            {
                var fls = e.Processor.GetService<ILocatingProvider>();
                var config = processor.Macro.Storage.Get<RegionConfig>();
                if (config is null || !fls.Locate(config))
                {
                    e.Cancel = true;
                    return;
                }
            }
        };
    }

    public static readonly MacroAction KillServerAction = new()
    {
        Text = "Kill ADB server",
        Description = "Runs \"adb kill-server\" via Command Prompt to kill the adb server.",
        Icon = "\uE756",
        Execute = args =>
        {
            if (!args.Options.TryGetValue(AdbService.ExePathKey, out var value) || value is not string exepath || !File.Exists(exepath))
            {
                args.Message = $"ADB executable file is not set or does not exist.";
                return;
            }

            Process.Start(new ProcessStartInfo()
            {
                FileName = exepath,
                Arguments = "kill-server",
            });

            args.Message = $"Killed adb server.";
        },
    };

}
