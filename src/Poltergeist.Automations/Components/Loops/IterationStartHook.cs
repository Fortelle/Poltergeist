using System;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationStartedHook : MacroHook
{
    public int Index { get; set; }
    public DateTime StartTime { get; set; }

    public IterationStartedHook(int index, DateTime startTime)
    {
        Index = index;
        StartTime = startTime;
    }
}
