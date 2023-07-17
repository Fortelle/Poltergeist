using System;

namespace Poltergeist.Automations.Processors.Events;

public class MacroCompletedEventArgs : EventArgs
{
    public EndReason Status { get; set; }
    public ProcessSummary Summary { get; set; }

    public CompletionAction CompleteAction { get; set; }
    public object? ActionArgument { get; set; }

    public bool IsSucceeded => Status == EndReason.Complete || Status == EndReason.Purposed;

    public Exception? Exception { get; set; }

    public MacroCompletedEventArgs(EndReason status, ProcessSummary summary)
    {
        Status = status;
        Summary = summary;
    }
}
