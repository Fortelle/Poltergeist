using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Macros.Loops;

public class LoopStartedHook : MacroHook
{
    public LoopPattern Pattern { get; init; }
    public int MaxIterations { get; init; }
    public TimeSpan MaxDuration { get; init; }
}
