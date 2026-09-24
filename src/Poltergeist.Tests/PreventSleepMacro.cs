using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Modules.Macros;

namespace Poltergeist.Tests;

[MacroInstance]
[ModuleDependency<OneshotModule>]
public class PreventSleepMacro : MacroBase
{
    public PreventSleepMacro() : base()
    {
        PreventsDisplaySleep = true;
    }

    [MacroHook]
    private static void OnOneshotSetup(IMacroProcessorShared _, OneshotSetupHook hooks)
    {
        hooks.ExecuteAsync = async (processor) =>
        {
            await Task.Delay(TimeSpan.FromHours(1), processor.CancellationToken);
        };
    }
}
