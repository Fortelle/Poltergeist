using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationEndedHook : MacroHook
{
    public int Index { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public IterationStatus Result { get; set; }

    public IterationEndedHook(int index, DateTime startTime, DateTime endTime, IterationStatus result)
    {
        Index = index;
        StartTime = startTime;
        EndTime = endTime;
        Result = result;
    }
}
