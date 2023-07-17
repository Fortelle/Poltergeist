using System;

namespace Poltergeist.Automations.Processors.Events;

public class MacroStartedEventArgs : EventArgs
{
    public required bool Started { get; init; }
    public required StartedAction[] StartedActions { get; init; }
}
