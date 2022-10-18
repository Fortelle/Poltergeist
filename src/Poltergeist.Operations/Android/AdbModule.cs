using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Operations.AndroidEmulators;

namespace Poltergeist.Operations.Android;

public class AdbModule : MacroModule
{
    public AdbModule()
    {

    }

    public override void OnMacroInitialize(IMacroInitializer macro)
    {
        macro.UserOptions.Add(new OptionItem<bool>(AdbService.AutoCloseKey, true)
        {
            DisplayLabel = "Auto close adb",
            Category = "ADB",
        });
    }


    public override void OnMacroConfigure(MacroServiceCollection services, IConfigureProcessor processor)
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

        options.Add(new FileOptionItem(AdbService.ExePathKey)
        {
            DisplayLabel = "Exe file",
            Category = "ADB",
        });
    }
}
