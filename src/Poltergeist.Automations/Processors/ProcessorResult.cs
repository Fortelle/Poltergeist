using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public class ProcessorResult
{
    public required EndReason Reason { get; init; }

    public required ProcessorReport Report { get; init; }

    public required IReadOnlyParameterValueCollection Output { get; init; }

    public Exception? Exception { get; init; }

    public bool IsSucceeded => Reason == EndReason.Complete;
}
