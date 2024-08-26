using System.Diagnostics;

namespace Poltergeist.Automations.Processors;

public class PauseProvider
{
    private volatile TaskCompletionSource<bool>? Source;

    private static readonly Task CompletedTask = Task.FromResult(true);

    public bool IsPaused => Source is not null;

    public async Task Pause()
    {
        Interlocked.CompareExchange(ref Source, new(), null);

        Debug.WriteLine("Paused");
        Debug.Flush();

        await (Source?.Task ?? CompletedTask);
    }

    public void Resume()
    {
        while (true)
        {
            var tcs = Source;
            if (tcs is null)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref Source, null, tcs) == tcs)
            {
                tcs.SetResult(true);
                break;
            }
        }
        Debug.WriteLine("Resumed");
    }

}
