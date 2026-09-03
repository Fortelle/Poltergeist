using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class PeriodicTrigger : MacroService
{
    public DateTime StartTime { get; private set; }

    public TimeSpan StartTimeElapsed { get; private set; }

    public TimeSpan Timeout { get; set; }

    public TimeSpan Interval { get; set; }

    public bool RunFirstExecutionImmediately { get; set; }

    private int TickCount = 0;

    private CancellationTokenSource? TimeoutCTS;

    private CancellationTokenSource? LinkedCTS;

    public PeriodicTrigger(MacroProcessor processor) : base(processor)
    {
    }

    public async Task<bool> WaitOne()
    {
        if (LinkedCTS is null)
        {
            if (Timeout != default)
            {
                TimeoutCTS = new CancellationTokenSource(Timeout);
                LinkedCTS = CancellationTokenSource.CreateLinkedTokenSource(Processor.CancellationToken, TimeoutCTS.Token);
            }
            else
            {
                LinkedCTS = CancellationTokenSource.CreateLinkedTokenSource(Processor.CancellationToken);
            }

            StartTime = DateTime.Now;
            StartTimeElapsed = Processor.GetElapsedTime();
        }

        if (RunFirstExecutionImmediately)
        {
            TickCount--;
        }

        var nextTimeSpan = StartTimeElapsed + Interval * (TickCount + 1);
        var nextTimeout = nextTimeSpan - Processor.GetElapsedTime();
        if (nextTimeout > TimeSpan.Zero)
        {
            await Task.Delay(nextTimeout, LinkedCTS.Token);
        }

        if (LinkedCTS.IsCancellationRequested)
        {
            return false;
        }

        TickCount++;

        return true;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        TimeoutCTS?.Dispose();
        LinkedCTS?.Dispose();
    }
}
