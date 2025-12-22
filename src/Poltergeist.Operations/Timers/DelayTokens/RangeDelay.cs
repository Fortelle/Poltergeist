using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Timers;

public class RangeDelay : DelayToken
{
    public int StartMilliseconds { get; }

    public int EndMilliseconds { get; }

    public bool? Floating { get; set; }

    public RangeDistributionType? RangeDistribution { get; set; }

    public RangeDelay(int startMilliseconds, int endMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(endMilliseconds, startMilliseconds, "End time must be greater than or equal to start time.");
        StartMilliseconds = startMilliseconds;
        EndMilliseconds = endMilliseconds;
    }

    public RangeDelay(TimeSpan start, TimeSpan end)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start, "End time must be greater than or equal to start time.");
        StartMilliseconds = (int)start.TotalMilliseconds;
        EndMilliseconds = (int)end.TotalMilliseconds;
    }

    public RangeDelay(TimeSpanRange range)
    {
        StartMilliseconds = (int)range.Start.TotalMilliseconds;
        EndMilliseconds = (int)range.End.TotalMilliseconds;
    }

    public const int MillisecondsPerSeconds = 1000;
    public const int MillisecondsPerMinute = 60 * MillisecondsPerSeconds;
    public const int MillisecondsPerHour = 60 * MillisecondsPerMinute;
    public const int MillisecondsPerDay = 24 * MillisecondsPerHour;

    public static RangeDelay FromDays(int startDays, int endDays) => new(startDays * MillisecondsPerDay, endDays * MillisecondsPerDay);
    public static RangeDelay FromDays(double startValue, double endValue) => new((int)(startValue * MillisecondsPerDay), (int)(endValue * MillisecondsPerDay));
    public static RangeDelay FromHours(int startHours, int endHours) => new(startHours * MillisecondsPerHour, endHours * MillisecondsPerHour);
    public static RangeDelay FromHours(double startValue, double endValue) => new((int)(startValue * MillisecondsPerHour), (int)(endValue * MillisecondsPerHour));
    public static RangeDelay FromMinutes(int startMinutes, int endMinutes) => new(startMinutes * MillisecondsPerMinute, endMinutes * MillisecondsPerMinute);
    public static RangeDelay FromMinutes(double startValue, double endValue) => new((int)(startValue * MillisecondsPerMinute), (int)(endValue * MillisecondsPerMinute));
    public static RangeDelay FromSeconds(int startSeconds, int endSeconds) => new(startSeconds * MillisecondsPerSeconds, endSeconds * MillisecondsPerSeconds);
    public static RangeDelay FromSeconds(double startValue, double endValue) => new((int)(startValue * MillisecondsPerSeconds), (int)(endValue * MillisecondsPerSeconds));
    public static RangeDelay FromMilliseconds(int startMilliseconds, int endMilliseconds) => new(startMilliseconds, endMilliseconds);
}
