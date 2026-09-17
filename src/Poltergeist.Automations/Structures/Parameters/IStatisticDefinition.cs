using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

public interface IStatisticDefinition : IParameterDefinition
{
    string? TargetKey { get; }

    bool IsGlobal { get; }

    bool TryUpdate(object? accumulatedValue, IReadOnlyParameterValueCollection report, [MaybeNullWhen(false)] out object? updatedValue);
}
