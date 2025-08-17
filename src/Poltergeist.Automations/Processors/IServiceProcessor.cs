namespace Poltergeist.Automations.Processors;

public interface IServiceProcessor : IUserProcessor
{
    ProcessorStatus Status { get; }

    public void RaiseEvent(ProcessorEvent type, EventArgs eventArgs);
    public TimeSpan GetElapsedTime();
}
