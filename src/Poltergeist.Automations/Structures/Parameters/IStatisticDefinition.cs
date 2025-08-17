using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Structures.Parameters;

public interface IStatisticDefinition : IParameterDefinition
{
    public string? TargetKey { get; }

    public bool TryUpdate(object? accumulatedValue, ProcessorReport report, [MaybeNullWhen(false)] out object? updatedValue);
}
