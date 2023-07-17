using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.Android;
using Poltergeist.Operations.BackgroundWindows;
using Poltergeist.Operations.ForegroundWindows;
using Poltergeist.Operations.Macros;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.AndroidEmulators;

public class EmulatorModule : MacroModule
{
    public const string InputModeKey = "adb.inputmode";

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

        macro.UserOptions.Add(new OptionItem<InputMode>(InputModeKey, InputMode.ADB_Only)
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

        var inputMode = processor.GetOption<InputMode>(InputModeKey);

        switch (inputMode)
        {
            case InputMode.ADB_Only:
                {
                    services.AddSingleton<TerminalService>();
                    services.AddSingleton<AdbService>();
                    services.AddSingleton<AdbInputService>();

                    services.AddSingleton<CapturingSource, AdbCapturingService>();

                    services.AddSingleton<IEmulatorInputSource, EmulatorAdbService>();
                }
                break;
            case InputMode.ADB_Background:
                {
                    services.AddSingleton<TerminalService>();
                    services.AddSingleton<AdbService>();
                    services.AddSingleton<AdbInputService>();

                    services.AddSingleton<BackgroundLocatingService>();
                    services.AddSingleton<CapturingSource, BackgroundCapturingService>();

                    services.AddSingleton<IEmulatorInputSource, EmulatorAdbService>();
                }
                break;
            case InputMode.Mouse:
                {
                    services.AddSingleton<ForegroundLocatingService>();
                    services.AddSingleton<ForegroundMouseService>();

                    services.AddSingleton<CapturingSource, ForegroundCapturingService>();
                    services.AddSingleton<IEmulatorInputSource, EmulatorMouseService>();
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
            var inputMode = e.Processor.GetOption<InputMode>(InputModeKey);

            if (inputMode is InputMode.ADB_Only or InputMode.ADB_Background)
            {
                var adb = e.Processor.GetService<AdbService>();
                if (!adb.Connect())
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (inputMode == InputMode.ADB_Background)
            {
                var bls = e.Processor.GetService<BackgroundLocatingService>();
                var config = processor.Macro.Storage.Get<RegionConfig>();
                if (!bls.Locate(config))
                {
                    e.Cancel = true;
                    return;
                }
            }
            else if (inputMode == InputMode.Mouse)
            {
                var fls = e.Processor.GetService<ForegroundLocatingService>();
                var config = processor.Macro.Storage.Get<RegionConfig>();
                if (!fls.Locate(config))
                {
                    e.Cancel = true;
                    return;
                }
            }
        };
    }

    public override void SetGlobalOptions(MacroOptions options)
    {
        base.SetGlobalOptions(options);

        options.Add(new OptionItem<string>(AdbService.IpAddressKey)
        {
            DisplayLabel = "IP Address",
            Category = "ADB",
        });

        options.Add(new PathOption(AdbService.ExePathKey)
        {
            DisplayLabel = "Exe file",
            Category = "ADB",
        });

        options.Add(new OptionItem<bool>(CapturingSource.PreviewCaptureKey)
        {
            DisplayLabel = "Preview captured image",
            Category = "Debug",
        });
    }

    public static readonly MacroAction KillServerAction = new()
    {
        Text = "Kill ADB server",
        Glyph = "\uE756",
        Execute = args =>
        {
            throw new NotImplementedException();

            //var path = ; // todo: how to access global options here?
            //if (!File.Exists(path))
            //{
            //    Debug.WriteLine($"File \"{path}\" does not exist.");
            //    return;
            //}

            //Process.Start(new ProcessStartInfo()
            //{
            //    FileName = path,
            //    Arguments = "kill-server",
            //});
        },
    };

    public enum InputMode
    {
        ADB_Only,
        ADB_Background,
        Mouse,
    }
}
