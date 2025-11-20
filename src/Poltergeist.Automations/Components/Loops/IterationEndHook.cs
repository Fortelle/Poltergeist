using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class IterationEndHook : MacroHook
{
    public int Index { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public TimeSpan Duration { get; init; }
    public WorkflowStepState State { get; init; }
}
