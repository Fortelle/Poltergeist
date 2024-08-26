using System.Diagnostics;

namespace Poltergeist.Automations.Utilities;

public static class DebugUtil
{
    public static TimeSpan[] Test(int count, params Action[] actions)
    {
        var times = new TimeSpan[actions.Length];

        for (var i = 0; i < actions.Length; i++)
        {
            var action = actions[i];
            action();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var j = 0; j < count; j++)
            {
                action();
            }
            stopwatch.Stop();
            times[i] = stopwatch.Elapsed;
        }
        return times;
    }

}
