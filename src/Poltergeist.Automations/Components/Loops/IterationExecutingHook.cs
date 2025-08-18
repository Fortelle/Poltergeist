using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationExecutingHook : MacroHook
{
    public int Index { get; init; }
    public bool IsInvalid { get; set; }
    public IterationResult Result { get; set; }
}
