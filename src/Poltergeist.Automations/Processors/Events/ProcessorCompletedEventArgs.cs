namespace Poltergeist.Automations.Processors;

public sealed class ProcessorCompletedEventArgs : EventArgs
{
    public required ProcessorResult Result { get; init; }
}
