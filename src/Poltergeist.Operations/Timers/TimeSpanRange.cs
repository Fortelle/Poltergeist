using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Operations.Timers;

public readonly struct TimeSpanRange : IEquatable<TimeSpanRange>
{
    public readonly TimeSpan Start { get; }

    public readonly TimeSpan End { get; }

    public TimeSpanRange()
    {
    }

    public TimeSpanRange(TimeSpan start, TimeSpan end)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start, "End time must be greater than or equal to start time.");
        Start = start;
        End = end;
    }

    public TimeSpanRange(TimeSpan start)
    {
        Start = start;
        End = start;
    }

    public bool Equals(TimeSpanRange other)
    {
        return other.Start.Equals(Start) && other.End.Equals(End);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is TimeSpanRange range && Equals(range);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start.GetHashCode(), End.GetHashCode());
    }

    public override string ToString()
    {
        return $"{Start}..{End}";
    }

    public static bool operator ==(TimeSpanRange left, TimeSpanRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TimeSpanRange left, TimeSpanRange right)
    {
        return !(left == right);
    }

    public static TimeSpanRange FromDays(int days1, int days2) => new(TimeSpan.FromDays(days1), TimeSpan.FromDays(days2));
    public static TimeSpanRange FromHours(int hours1, int hours2) => new(TimeSpan.FromHours(hours1), TimeSpan.FromHours(hours2));
    public static TimeSpanRange FromMinutes(long minutes1, long minutes2) => new(TimeSpan.FromMinutes(minutes1), TimeSpan.FromMinutes(minutes2));
    public static TimeSpanRange FromSeconds(long seconds1, long seconds2) => new(TimeSpan.FromSeconds(seconds1), TimeSpan.FromSeconds(seconds2));
    public static TimeSpanRange FromMilliseconds(long milliseconds1, long milliseconds2) => new(TimeSpan.FromMilliseconds(milliseconds1), TimeSpan.FromMilliseconds(milliseconds2));
    public static TimeSpanRange FromMicroseconds(long microseconds1, long microseconds2) => new(TimeSpan.FromMicroseconds(microseconds1), TimeSpan.FromMicroseconds(microseconds2));
}
