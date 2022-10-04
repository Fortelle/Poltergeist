using System;

namespace Poltergeist.Automations.Processors.Events;

public class MacroCompletedEventArgs : EventArgs
{
    public EndReason Status;
    public ProcessReport Summary;

    public MacroCompletedEventArgs(EndReason status, ProcessReport summary)
    {
        Status = status;
        Summary = summary;
    }
}
