using System.Threading.Channels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class EventTrigger : MacroService
{
    public TimeSpan Timeout { get; set; }

    private CancellationTokenSource? TimeoutCTS;

    private CancellationTokenSource? LinkedCTS;

    public EventTrigger(MacroProcessor processor) : base(processor)
    {
    }

    private Channel<bool>? Queue;

    public void Notify()
    {
        Queue!.Writer.TryWrite(true);
    }

    public async Task<bool> WaitOne()
    {
        if (LinkedCTS is null)
        {
            Queue = Channel.CreateUnbounded<bool>();

            if (Timeout != default)
            {
                TimeoutCTS = new CancellationTokenSource(Timeout);
                LinkedCTS = CancellationTokenSource.CreateLinkedTokenSource(Processor.CancellationToken, TimeoutCTS.Token);
            }
            else
            {
                LinkedCTS = CancellationTokenSource.CreateLinkedTokenSource(Processor.CancellationToken);
            }
        }

        await Queue!.Reader.ReadAsync(LinkedCTS.Token);

        if (LinkedCTS.IsCancellationRequested)
        {
            return false;
        }

        return true;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        TimeoutCTS?.Dispose();
        LinkedCTS?.Dispose();
    }
}
