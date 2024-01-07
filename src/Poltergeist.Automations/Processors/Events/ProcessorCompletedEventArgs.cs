namespace Poltergeist.Automations.Processors;

public class ProcessorCompletedEventArgs : EventArgs
{
    public EndReason Status { get; set; }
    public ProcessHistoryEntry History { get; set; }

    public CompletionAction CompleteAction { get; set; }
    public object? ActionArgument { get; set; }

    public bool IsSucceeded => Status == EndReason.Complete;

    public Exception? Exception { get; set; }

    public ProcessorCompletedEventArgs(EndReason status, ProcessHistoryEntry history)
    {
        Status = status;
        History = history;
    }
}
