using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public interface IFrontProcessor : IProcessor, IDisposable
{
    public IFrontMacro Macro { get; }

    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public void Start();
    public void Execute();
    public Task ExecuteAsync();
    public void Stop(AbortReason reason);
    public Task Pause(PauseReason reason);
    public void Terminate();
    public void RaiseEvent(ProcessorEvent type, EventArgs eventArgs);
    public void ReceiveMessage(Dictionary<string, string> paramaters);
    public void Resume();

    public event EventHandler<ProcessorLaunchedEventArgs>? Launched;
    public event EventHandler<ProcessorCompletedEventArgs>? Completed;
    public event EventHandler<PanelCreatedEventArgs>? PanelCreated;
    public event EventHandler<InteractingEventArgs>? Interacting;
}
