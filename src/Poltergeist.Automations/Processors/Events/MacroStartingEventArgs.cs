using System;

namespace Poltergeist.Automations.Processors.Events;

public class MacroStartingEventArgs : EventArgs
{
    public DateTime StartTime;

    public MacroStartingEventArgs(DateTime startTime)
    {
        StartTime = startTime;
    }
}
