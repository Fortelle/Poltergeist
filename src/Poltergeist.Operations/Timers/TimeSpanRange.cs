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

    public static TimeSpanRange FromMilliseconds(int milliseconds1, int milliseconds2) => new(TimeSpan.FromMilliseconds(milliseconds1), TimeSpan.FromMilliseconds(milliseconds2));

    public static TimeSpanRange FromSeconds(int seconds1, int seconds2) => new(TimeSpan.FromSeconds(seconds1), TimeSpan.FromSeconds(seconds2));
}
