using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Structures.Parameters;

public class StatisticDefinition<T> : ParameterDefinition<T>, IStatisticDefinition
{
    public delegate bool TryUpdateCallback(T? accumulatedValue, ProcessorReport report, [MaybeNullWhen(false)] out T updatedValue);
    public delegate T UpdateCallback(T? accumulatedValue, T? currentValue);

    public StatisticDefinition(string key) : base(key)
    {
    }

    public StatisticDefinition(string key, T? value) : base(key, value)
    {
    }

    public string? TargetKey { get; init; }

    public TryUpdateCallback? TryUpdate { get; init; }

    public UpdateCallback? Update { get; init; }

    bool IStatisticDefinition.TryUpdate(object? accumulatedValue, ProcessorReport report, [MaybeNullWhen(false)] out object? updatedValue)
    {
        var accumulatedValueT = accumulatedValue is T x ? x : default;

        if (TryUpdate is not null)
        {
            if (TryUpdate(accumulatedValueT, report, out var value))
            {
                updatedValue = value;
                return true;
            }
            else
            {
                updatedValue = default;
                return false;
            }
        }
        else if (Update is not null)
        {
            var currentValue = TargetKey is not null && report.TryGetValue(TargetKey, out var targetValue) && targetValue is T targetValueT ? targetValueT : default;
            updatedValue = Update(accumulatedValueT, currentValue);
            return true;
        }
        else
        {
            throw new ArgumentException("Both TryUpdate and Update callbacks are null. At least one must be provided.");
        }
    }
}
