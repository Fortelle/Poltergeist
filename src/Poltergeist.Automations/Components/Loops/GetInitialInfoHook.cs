using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Automations.Components.Loops;

public class GetInitialInfoHook : MacroHook
{
    public ProgressInstrumentInfo? Info { get; set; }
}
