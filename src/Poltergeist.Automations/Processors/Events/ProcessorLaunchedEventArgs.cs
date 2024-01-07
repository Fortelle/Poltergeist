namespace Poltergeist.Automations.Processors;

public class ProcessorLaunchedEventArgs : EventArgs
{
    public DateTime StartTime;

    public ProcessorLaunchedEventArgs(DateTime startTime)
    {
        StartTime = startTime;
    }
}
