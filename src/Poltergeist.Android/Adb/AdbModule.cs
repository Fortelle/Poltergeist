using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Emulators;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Android.Adb;

public class AdbModule : MacroModule
{
    static AdbModule()
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
    }

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

}
