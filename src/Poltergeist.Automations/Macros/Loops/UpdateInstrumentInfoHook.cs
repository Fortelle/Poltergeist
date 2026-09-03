using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Automations.Macros.Loops;

public class UpdateInstrumentInfoHook : MacroHook
{
    public int Index { get; init; }
    public ProgressInstrumentInfo? Info { get; set; }
}
