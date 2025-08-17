namespace Poltergeist.Automations.Processors;

public sealed class ProcessorCompletedEventArgs : EventArgs
{
    public required EndReason Reason { get; init; }

    public required ProcessorResult Result { get; init; }

    public bool IsSucceeded => Reason == EndReason.Complete;
}
