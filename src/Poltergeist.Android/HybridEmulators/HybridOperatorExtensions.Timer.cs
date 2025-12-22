using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.HybridEmulators;

public static partial class HybridOperatorExtensions
{
    public static IHybridOperator Delay(this IHybridOperator @operator, int milliseconds, DelayOptions? options = null)
    {
        @operator.Timer.Delay(new CoarseDelay(milliseconds), options);
        return @operator;
    }

    public static IHybridOperator Delay(this IHybridOperator @operator, int min, int max, DelayOptions? options = null)
    {
        @operator.Timer.Delay(new RangeDelay(min, max), options);
        return @operator;
    }

    public static IHybridOperator Delay(this IHybridOperator @operator, TimeSpan timespan, DelayOptions? options = null)
    {
        @operator.Timer.Delay(new CoarseDelay(timespan), options);
        return @operator;
    }
}
