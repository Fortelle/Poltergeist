namespace Poltergeist.Automations.Processors;

public interface IServiceProcessor : IUserProcessor
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string? Comment { get; set; }

    public void RaiseEvent(ProcessorEvent type, EventArgs eventArgs);
    public void RaiseAction(Action action);
    public void CheckCancel();
}
