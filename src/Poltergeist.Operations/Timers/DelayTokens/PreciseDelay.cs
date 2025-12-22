namespace Poltergeist.Operations.Timers;

public class PreciseDelay : DelayToken
{
    public int Milliseconds { get; }

    public PreciseDelay(int milliseconds)
    {
        Milliseconds = milliseconds;
    }

    public PreciseDelay(TimeSpan timeout)
    {
        Milliseconds = (int)timeout.TotalMilliseconds;
    }
}
