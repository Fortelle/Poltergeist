using System;

namespace Poltergeist.Automations.Processors.Events;

public class MacroStartedEventArgs : EventArgs
{
    public bool Started;
    public StartedAction[] StartedActions;

    public MacroStartedEventArgs()
    {
    }
}
