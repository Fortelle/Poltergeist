namespace Poltergeist.Automations.Components.Interactions;

public class ProgressModel : InteractionModel
{
    public string? Title { get; init; }
    public bool IsOn { get; init; }
    public CancellationTokenSource? CancellationTokenSource { get; init; }
}
