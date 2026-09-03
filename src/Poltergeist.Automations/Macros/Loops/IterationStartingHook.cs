using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Macros.Loops;

public class IterationStartingHook : MacroHook
{
    public int Index { get; init; }

    public bool Cancel { get; set; }
}
