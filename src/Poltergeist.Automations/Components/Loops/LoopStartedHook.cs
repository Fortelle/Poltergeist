using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class LoopStartedHook : MacroHook
{
    public bool IsCancelled { get; init; }
    public string? CancelReason { get; init; }
}
