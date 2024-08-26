using System.Diagnostics;

namespace Poltergeist.Automations.Utilities;

public static class TimerUtil
{
    public static TimeSpan Time(Action action)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

}
