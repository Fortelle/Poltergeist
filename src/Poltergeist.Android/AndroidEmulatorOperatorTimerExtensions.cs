using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Macros;

public static class AndroidEmulatorOperatorTimerExtensions
{
    public static IOperator Delay(this IOperator @operator, int milliseconds, DelayOptions? options = null)
    {
        @operator.Timer.Delay(milliseconds, options);
        return @operator;
    }

    public static IOperator Delay(this IOperator @operator, int min, int max, DelayOptions? options = null)
    {
        @operator.Timer.Delay(min, max, options);
        return @operator;
    }
}
