using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Macros.Loops;

public class IterationErrorHook : MacroHook
{
    public int Index { get; init; }

    public required Exception Exception { get; init; }
}
