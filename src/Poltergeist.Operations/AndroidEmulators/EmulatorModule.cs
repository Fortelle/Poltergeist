using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Components.Loops;
using Poltergeist.Operations.Android;
using Poltergeist.Operations.BackgroundWindows;
using Poltergeist.Operations.ForegroundWindows;
using Poltergeist.Operations.Macros;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.AndroidEmulators;

public class EmulatorModule : MacroModule
{
    public enum InputMode
    {
        ADB_Only,
        ADB_Background,
        Mouse,
    }

    public EmulatorModule()
    {

    }

    public override void OnMacroInitialize(IMacroInitializer macro)
    {
        macro.UserOptions.Add(new OptionItem<bool>(AdbService.AutoCloseKey, true)
        {
            DisplayLabel = "Auto close adb",
            Category = "ADB",
        });

        macro.UserOptions.Add(new OptionItem<InputMode>("adb.inputmode", InputMode.ADB_Only)
        {
            DisplayLabel = "Input mode",
            Category = "ADB",
        });
    }


    public override void OnMacroConfigure(MacroServiceCollection services, IConfigureProcessor processor)
    {
        base.OnMacroConfigure(services, processor);

        services.AddSingleton<TimerService>();
        services.AddSingleton<AndroidEmulatorOperator>();
        services.AddSingleton<RandomEx>();
        services.AddSingleton<DistributionService>();

        var inputMode = processor.GetOption<InputMode>("adb.inputmode");

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

    public override void OnMacroProcess(MacroProcessor processor)
    {
        base.OnMacroProcess(processor);

        var repeats = processor.GetService<RepeatService>();

        repeats.BeginProc += (e) =>
        {
            var inputMode = e.Processor.GetOption<InputMode>("adb.inputmode");

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

        options.Add(new FileOptionItem(AdbService.ExePathKey)
        {
            DisplayLabel = "Exe file",
            Category = "ADB",
        });

        options.Add(new OptionItem<bool>("capture_preview")
        {
            DisplayLabel = "Preview captured image",
            Category = "Debug",
        });
    }
}
