namespace Poltergeist.Automations.Components.Loops;

public class IterationData
{
    public required int Index { get; init; }
    public bool IsInvalid { get; set; }
    public IterationResult Result { get; set; }
}
