using System;

namespace Poltergeist.Automations.Processors.Events;

public class MacroCompletedEventArgs : EventArgs
{
    public ProcessEndReason Status;
    public ProcessReport Summary;

    public MacroCompletedEventArgs(ProcessEndReason status, ProcessReport summary)
    {
        Status = status;
        Summary = summary;
    }
}
