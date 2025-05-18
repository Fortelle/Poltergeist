using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Automations.Components.Loops;

public class UpdatetInstrumentInfoHook : MacroHook
{
    public int Index { get; init; }
    public ProgressInstrumentInfo? Info { get; set; }
}
