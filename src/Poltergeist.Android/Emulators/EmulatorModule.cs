using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Adb;
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
    public const string CapturingModeKey = "adb.capturingmode";
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

        macro.UserOptions.Add(new OptionDefinition<EmulatorOperationMode>(CapturingModeKey, EmulatorOperationMode.ADB)
        {
            DisplayLabel = "Capturing mode",
            Category = "ADB",
        });

        macro.UserOptions.Add(new OptionDefinition<EmulatorOperationMode>(InputModeKey, EmulatorOperationMode.ADB)
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
        processor.Services.AddSingleton<EmulatorService>();

        var capturingMode = processor.Options.Get<EmulatorOperationMode>(CapturingModeKey);
        var inputMode = processor.Options.Get<EmulatorOperationMode>(InputModeKey);

        if (inputMode == EmulatorOperationMode.Foreground || capturingMode == EmulatorOperationMode.Foreground)
        {
            processor.Services.AddSingleton<ForegroundLocatingService>();
            processor.Services.AddSingleton<ILocatingProvider>(x => x.GetRequiredService<ForegroundLocatingService>());
        }
        if (inputMode == EmulatorOperationMode.Background || capturingMode == EmulatorOperationMode.Background)
        {
            processor.Services.AddSingleton<BackgroundLocatingService>();
            processor.Services.AddSingleton<ILocatingProvider>(x => x.GetRequiredService<BackgroundLocatingService>());
        }
        if (inputMode == EmulatorOperationMode.ADB || capturingMode == EmulatorOperationMode.ADB)
        {
            processor.Services.AddSingleton<TerminalService>();
            processor.Services.AddSingleton<AdbService>();
        }
        
        if (capturingMode == EmulatorOperationMode.Foreground)
        {
            processor.Services.AddSingleton<CapturingProvider, ForegroundCapturingService>();
        }
        if (capturingMode == EmulatorOperationMode.Background)
        {
            processor.Services.AddSingleton<CapturingProvider, BackgroundCapturingService>();
        }
        if (capturingMode == EmulatorOperationMode.ADB)
        {
            processor.Services.AddSingleton<CapturingProvider, AdbCapturingService>();
        }
        if (capturingMode == EmulatorOperationMode.None)
        {
            processor.Services.AddSingleton<CapturingProvider, EmptyCapturingService>();
        }

        if (inputMode == EmulatorOperationMode.Foreground)
        {
            processor.Services.AddSingleton<ForegroundMouseService>();
            processor.Services.AddSingleton<IEmulatorInputProvider, EmulatorForegroundMouseService>();
        }
        if (inputMode == EmulatorOperationMode.Background)
        {
            processor.Services.AddSingleton<BackgroundMouseService>();
            processor.Services.AddSingleton<IEmulatorInputProvider, EmulatorBackgroundMouseService>();
        }
        if (inputMode == EmulatorOperationMode.ADB)
        {
            processor.Services.AddSingleton<AdbInputService>();
            processor.Services.AddSingleton<IEmulatorInputProvider, EmulatorAdbService>();
        }
        if (inputMode == EmulatorOperationMode.None)
        {
            processor.Services.AddSingleton<IEmulatorInputProvider, EmulatorEmptyInputService>();
        }
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        var config = processor.Macro.Storage.Get<RegionConfig>();
        if (config != null && !processor.SessionStorage.Contains("window_region_config")) {
            processor.SessionStorage.Add("window_region_config", config);
        }
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
