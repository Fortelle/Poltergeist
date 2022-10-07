using System;

namespace Poltergeist.Automations.Processors.Events;

public class MacroCompletedEventArgs : EventArgs
{
    public EndReason Status;
    public ProcessReport Summary;

    public CompleteAction CompleteAction;
    public object ActionArgument;

    public bool IsSucceeded => Status == EndReason.Complete || Status == EndReason.Purposed;

    public MacroCompletedEventArgs(EndReason status, ProcessReport summary)
    {
        Status = status;
        Summary = summary;
    }
}
