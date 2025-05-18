namespace Poltergeist.Automations.Processors;

public sealed class ProcessorCompletedEventArgs : EventArgs
{
    public EndReason Reason { get; init; }

    public ProcessHistoryEntry? HistoryEntry { get; init; }
    public CompletionAction CompletionAction { get; init; }
    public Exception? Exception { get; init; }

    public bool IsSucceeded => Reason == EndReason.Complete;
}
