using Poltergeist.Android.Adb;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Operations;
using Poltergeist.Operations.Background;
using Poltergeist.Operations.Foreground;

namespace Poltergeist.Android.Emulators;

public class EmulatorService(MacroProcessor processor) : MacroService(processor)
{
    public void Connect()
    {
        var capturingMode = Processor.Options.Get<EmulatorOperationMode>(EmulatorModule.CapturingModeKey);
        var inputMode = Processor.Options.Get<EmulatorOperationMode>(EmulatorModule.InputModeKey);
        var config = Processor.SessionStorage.Get<RegionConfig>("window_region_config");

        var hasAdbMode = capturingMode == EmulatorOperationMode.ADB || inputMode == EmulatorOperationMode.ADB;
        var hasForegroundMode = capturingMode == EmulatorOperationMode.Foreground || inputMode == EmulatorOperationMode.Foreground;
        var hasBackgroundMode = capturingMode == EmulatorOperationMode.Background || inputMode == EmulatorOperationMode.Background;

        if (hasAdbMode)
        {
            var adb = Processor.GetService<AdbService>();
            if (!adb.Connect())
            {
                throw new Exception("Failed to connect to adb server.");
            }
        }

        if (hasForegroundMode)
        {
            var locatingService = Processor.GetService<ForegroundLocatingService>();
            if (config is null || !locatingService.Locate(config))
            {
                throw new Exception("Failed to located the emulator window.");
            }
        }

        if (hasBackgroundMode)
        {
            var locatingService = Processor.GetService<BackgroundLocatingService>();
            if (config is null || !locatingService.Locate(config))
            {
                throw new Exception("Failed to find the emulator window.");
            }
        }
    }

    public void Disconnect()
    {
        var capturingMode = Processor.Options.Get<EmulatorOperationMode>(EmulatorModule.CapturingModeKey);
        var inputMode = Processor.Options.Get<EmulatorOperationMode>(EmulatorModule.InputModeKey);

        var hasAdbMode = capturingMode == EmulatorOperationMode.ADB || inputMode == EmulatorOperationMode.ADB;

        if (hasAdbMode)
        {
            var adb = Processor.GetService<AdbService>();
            adb.Close();
        }
    }

}
