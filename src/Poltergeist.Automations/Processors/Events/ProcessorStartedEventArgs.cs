namespace Poltergeist.Automations.Processors;

public class ProcessorStartedEventArgs : EventArgs
{
    public required StartedAction[] StartedActions { get; init; }
}
