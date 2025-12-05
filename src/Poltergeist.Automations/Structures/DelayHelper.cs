namespace Poltergeist.Automations.Structures;

public class DelayHelper : IDisposable
{
    private readonly TimeSpan _delay;
    private readonly Action _action;
    private readonly Lock _lock = new();

    private CancellationTokenSource? _cts;

    public DelayHelper(TimeSpan delay, Action action)
    {
        _delay = delay;
        _action = action;
    }

    public void Start()
    {
        lock (_lock)
        {
            CancelInternal();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_delay, token);
                    if (!token.IsCancellationRequested)
                    {
                        _action();
                    }
                }
                catch (TaskCanceledException)
                {
                }
            }, token);
        }
    }

    public void TriggerNow()
    {
        lock (_lock)
        {
            CancelInternal();
        }

        _action();
    }

    public void Cancel()
    {
        lock (_lock)
        {
            CancelInternal();
        }
    }

    private void CancelInternal()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public void Dispose()
    {
        Cancel();
        GC.SuppressFinalize(this);
    }
}
