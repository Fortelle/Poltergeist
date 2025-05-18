using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationExecutingHook : MacroHook
{
    public int Index { get; init; }
    public IterationResult Result { get; set; }
}
