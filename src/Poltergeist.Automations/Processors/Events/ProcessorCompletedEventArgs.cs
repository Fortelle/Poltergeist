namespace Poltergeist.Automations.Processors;

public sealed class ProcessorCompletedEventArgs : EventArgs
{
    public required EndReason Status { get; init; }
    public required ProcessHistoryEntry HistoryEntry { get; init; }

    public CompletionAction CompletionAction { get; init; }
    public Exception? Exception { get; init; }

    public bool IsSucceeded => Status == EndReason.Complete;
}
