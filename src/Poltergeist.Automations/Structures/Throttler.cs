
namespace Poltergeist.Automations.Structures;

public class Throttler
{
    private readonly Action Action;
    private readonly long IntervalMilliseconds;
    private long LastTriggerTick;
    private readonly Lock _lock = new();

    public Throttler(Action action, TimeSpan interval)
    {
        Action = action;
        IntervalMilliseconds = (long)interval.TotalMilliseconds;
    }

    public void Trigger(bool forceExecute = false)
    {
        var now = Environment.TickCount64;
        var shouldExecute = false;

        lock (_lock)
        {
            if (forceExecute || (now - LastTriggerTick >= IntervalMilliseconds))
            {
                LastTriggerTick = now;
                shouldExecute = true;
            }
        }

        if (shouldExecute)
        {
            Action();
        }
    }
}
