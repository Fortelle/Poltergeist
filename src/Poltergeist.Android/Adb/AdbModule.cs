using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Emulators;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Android.Adb;

public class AdbModule : MacroModule
{
    public AdbModule()
    {

    }

    public override void OnMacroInitialized(IMacroInitializer macro)
    {
        macro.UserOptions.Add(new OptionItem<bool>(AdbService.KeepAliveKey, true)
        {
            DisplayLabel = "Auto close adb",
            Category = "ADB",
        });
    }


    public override void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
        services.AddSingleton<TerminalService>();
        services.AddTransient<EmulatorAdbService>();

        services.AddSingleton<AdbService>();
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
    }
}
