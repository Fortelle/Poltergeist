using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Emulators;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Android.Adb;

public class AdbModule : MacroModule
{
    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.OptionDefinitions.Add(new OptionDefinition<string>(AdbService.IpAddressKey)
        {
            DisplayLabel = "IP Address",
            Category = "ADB",
            IsGlobal = true,
        });

        macro.OptionDefinitions.Add(new PathOption(AdbService.ExePathKey)
        {
            DisplayLabel = "Exe file",
            Category = "ADB",
            IsGlobal = true,
        });

        macro.OptionDefinitions.Add(new OptionDefinition<bool>(AdbService.KeepAliveKey, true)
        {
            DisplayLabel = "Auto close adb",
            Category = "ADB",
        });
    }

    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<TerminalService>();
        processor.Services.AddTransient<EmulatorAdbService>();

        processor.Services.AddSingleton<AdbService>();
    }

}
