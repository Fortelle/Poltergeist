using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Timers;

public class CoarseDelay : DelayToken
{
    public int Milliseconds { get; }

    public bool? Floating { get; set; }

    public (double, double)? FloatingRange { get; set; }

    public RangeDistributionType? FloatDistribution { get; set; }

    public CoarseDelay(int milliseconds)
    {
        Milliseconds = milliseconds;
    }

    public CoarseDelay(TimeSpan timeout)
    {
        Milliseconds = (int)timeout.TotalMilliseconds;
    }

    public const int MillisecondsPerSeconds = 1000;
    public const int MillisecondsPerMinute = 60 * MillisecondsPerSeconds;
    public const int MillisecondsPerHour = 60 * MillisecondsPerMinute;
    public const int MillisecondsPerDay = 24 * MillisecondsPerHour;

    public static CoarseDelay FromDays(int days) => new(days * MillisecondsPerDay);
    public static CoarseDelay FromDays(double value) => new((int)(value * MillisecondsPerDay));
    public static CoarseDelay FromHours(int hours) => new(hours * MillisecondsPerHour);
    public static CoarseDelay FromHours(double value) => new((int)(value * MillisecondsPerHour));
    public static CoarseDelay FromMinutes(int minutes) => new(minutes * MillisecondsPerMinute);
    public static CoarseDelay FromMinutes(double value) => new((int)(value * MillisecondsPerMinute));
    public static CoarseDelay FromSeconds(int seconds) => new(seconds * MillisecondsPerSeconds);
    public static CoarseDelay FromSeconds(double value) => new((int)(value * MillisecondsPerSeconds));
    public static CoarseDelay FromMilliseconds(int milliseconds) => new(milliseconds);
}
