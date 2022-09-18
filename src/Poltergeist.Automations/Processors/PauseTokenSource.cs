using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Poltergeist.Automations.Processors;

public class PauseTokenSource
{
    private volatile TaskCompletionSource<bool> _paused;

    public PauseToken Token => new PauseToken(this);

    internal Task WaitWhilePausedAsync()
    {
        var cur = _paused;
        return cur != null ? cur.Task : s_completedTask;
    }

    internal static readonly Task s_completedTask = Task.FromResult(true);

    public bool IsPaused
    {
        get => _paused != null;
        set
        {
            if (value)
            {
                Interlocked.CompareExchange(ref _paused, new TaskCompletionSource<bool>(), null);
            }
            else
            {
                Debug.WriteLine("Paused");
                Debug.Flush();
                while (true)
                {
                    var tcs = _paused;
                    if (tcs == null) return;
                    if (Interlocked.CompareExchange(ref _paused, null, tcs) == tcs)
                    {
                        tcs.SetResult(true);
                        break;
                    }
                }
                Debug.WriteLine("Resumed");
            }
        }
    }
}
