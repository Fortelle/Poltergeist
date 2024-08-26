namespace Poltergeist.Automations.Processors;

public class ProcessorLaunchedEventArgs(DateTime startTime) : EventArgs
{
    public DateTime StartTime => startTime;
}
