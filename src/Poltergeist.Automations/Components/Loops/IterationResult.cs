namespace Poltergeist.Automations.Components.Loops;

public class IterationResult
{
    public int Index { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public IterationStatus Status { get; set; }
}
