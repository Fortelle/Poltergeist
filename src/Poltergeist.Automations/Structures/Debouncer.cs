namespace Poltergeist.Automations.Structures;

public class Debouncer : IDisposable
{
    private readonly Action Action;
    private readonly TimeSpan Delay;
    private readonly TimeSpan? MaxDelay;
    private readonly bool ImmediateFirst;

    private CancellationTokenSource? Cts;
    private long? FirstTriggerTick = null;
    private bool HasExecutedAtLeastOnce = false;

    private readonly Lock _lock = new();

    private bool IsDisposed;

    public Debouncer(Action action, TimeSpan delay, bool immediateFirst = false, TimeSpan? maxDelay = null)
    {
        Action = action;
        Delay = delay;
        ImmediateFirst = immediateFirst;
        MaxDelay = maxDelay;
    }

    public void Trigger(bool forceExecute = false)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        CancellationTokenSource? localCts;
        TimeSpan effectiveDelay;

        lock (_lock)
        {
            Cts?.Cancel();

            if (forceExecute)
            {
                HasExecutedAtLeastOnce = true;
                FirstTriggerTick = null;
                Cts = null;
                Action();
                return;
            }

            if (ImmediateFirst && !HasExecutedAtLeastOnce)
            {
                HasExecutedAtLeastOnce = true;
                Action();
                return;
            }

            FirstTriggerTick ??= Environment.TickCount64;
            var elapsed = TimeSpan.FromMicroseconds(Environment.TickCount64 - FirstTriggerTick.Value);

            effectiveDelay = Delay;
            if (MaxDelay.HasValue && elapsed + Delay > MaxDelay.Value)
            {
                effectiveDelay = MaxDelay.Value - elapsed;
                if (effectiveDelay < TimeSpan.Zero)
                {
                    effectiveDelay = TimeSpan.Zero;
                }
            }

            Cts = new CancellationTokenSource();
            localCts = Cts;
        }

        var token = localCts.Token;

        Task.Run(() =>
        {
            try
            {
                Task.Delay(effectiveDelay, token).Wait(token);
                lock (_lock)
                {
                    if (Cts != localCts || token.IsCancellationRequested)
                    {
                        return;
                    }

                    HasExecutedAtLeastOnce = true;
                    FirstTriggerTick = null;
                    Cts = null;
                    Action();
                }
            }
            catch (OperationCanceledException) { }
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            Cts?.Cancel();
            Cts = null;
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
