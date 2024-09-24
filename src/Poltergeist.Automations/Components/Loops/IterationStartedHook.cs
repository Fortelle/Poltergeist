using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationStartedHook(int index, DateTime startTime) : MacroHook
{
    public int Index => index;
    public DateTime StartTime => startTime;
}
