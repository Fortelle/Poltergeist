using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationStartingHook : MacroHook
{
    public required int Index { get; init; }
    public required DateTime StartTime { get; init; }
}
