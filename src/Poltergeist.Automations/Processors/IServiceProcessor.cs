namespace Poltergeist.Automations.Processors;

public interface IServiceProcessor : IUserProcessor
{
    ProcessorStatus Status { get; }

    void RaiseEvent(ProcessorEvent type, EventArgs eventArgs);
    TimeSpan GetElapsedTime();
}
