namespace Poltergeist.Automations.Processors;

public interface IServiceProcessor : IUserProcessor
{
    public DateTime StartTime { get; }
    public string? Comment { get; set; }
    ProcessorStatus Status { get; }

    public void RaiseEvent(ProcessorEvent type, EventArgs eventArgs);
    public void RaiseAction(Action action);
    public TimeSpan GetElapsedTime();
}
