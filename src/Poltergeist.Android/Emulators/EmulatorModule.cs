using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Maths;
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
        GlobalOptions.Add(new OptionItem<string>(AdbService.IpAddressKey)
        {
            DisplayLabel = "IP Address",
            Category = "ADB",
        });

        GlobalOptions.Add(new PathOption(AdbService.ExePathKey)
        {
            DisplayLabel = "Exe file",
            Category = "ADB",
        });

        GlobalOptions.Add(new OptionItem<bool>(CapturingProvider.PreviewCaptureKey)
        {
            DisplayLabel = "Preview captured image",
            Category = "Debug",
        });
    }

    public EmulatorModule()
    {

    }

    public override void OnMacroInitialized(IMacroInitializer macro)
    {
        macro.UserOptions.Add(new OptionItem<bool>(AdbService.KeepAliveKey, true)
        {
            DisplayLabel = "Keep adb server alive",
            Description = "Skips killing the adb server when the macro is completed. " +
                "This helps when you are planning to launch the macro frequently in a short time. " +
                "You can use the \"kill-server\" action to kill the adb server manually.",
            Category = "ADB",
        });

        macro.UserOptions.Add(new OptionItem<InputMode>(InputModeKey, InputMode.ADB)
        {
            DisplayLabel = "Input mode",
            Category = "ADB",
        });

        macro.Actions.Add(KillServerAction);
    }


    public override void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
        base.OnMacroConfiguring(services, processor);

        services.AddSingleton<TimerService>();
        services.AddSingleton<AndroidEmulatorOperator>();
        services.AddSingleton<RandomEx>();
        services.AddSingleton<DistributionService>();

        var inputMode = processor.Options.Get<InputMode>(InputModeKey);

        switch (inputMode)
        {
            case InputMode.ADB:
                {
                    services.AddSingleton<TerminalService>();
                    services.AddSingleton<AdbService>();
                    services.AddSingleton<AdbInputService>();

                    services.AddSingleton<CapturingProvider, AdbCapturingService>();

                    services.AddSingleton<IEmulatorInputProvider, EmulatorAdbService>();
                }
                break;
            case InputMode.ADB_Background:
                {
                    services.AddSingleton<TerminalService>();
                    services.AddSingleton<AdbService>();
                    services.AddSingleton<AdbInputService>();

                    services.AddSingleton<BackgroundLocatingService>();
                    services.AddSingleton<ILocatingProvider>(x => x.GetRequiredService<BackgroundLocatingService>());
                    services.AddSingleton<CapturingProvider, BackgroundCapturingService>();

                    services.AddSingleton<IEmulatorInputProvider, EmulatorAdbService>();
                }
                break;
            case InputMode.Mouse:
                {
                    services.AddSingleton<ForegroundLocatingService>();
                    services.AddSingleton<ILocatingProvider>(x => x.GetRequiredService<ForegroundLocatingService>());
                    services.AddSingleton<ForegroundMouseService>();

                    services.AddSingleton<CapturingProvider, ForegroundCapturingService>();
                    services.AddSingleton<IEmulatorInputProvider, EmulatorMouseService>();
                }
                break;
        }
    }

    public override void OnMacroProcessing(MacroProcessor processor)
    {
        base.OnMacroProcessing(processor);

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
                if (!fls.Locate(config))
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
        Glyph = "\uE756",
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

    public enum InputMode
    {
        ADB,
        ADB_Background,
        Mouse,
    }
}
