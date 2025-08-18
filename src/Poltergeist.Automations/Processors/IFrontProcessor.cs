using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public interface IFrontProcessor : IProcessor, IDisposable
{
    IFrontMacro Macro { get; }

    void Start();
    ProcessorResult Execute();
    Task<ProcessorResult> ExecuteAsync();
    void Stop(AbortReason reason);
    Task Pause(PauseReason reason);
    void Terminate();
    void RaiseEvent(ProcessorEvent type, EventArgs eventArgs);
    void ReceiveMessage(Dictionary<string, string> paramaters);
    void Resume();

    event EventHandler<ProcessorLaunchedEventArgs>? Launched;
    event EventHandler<ProcessorCompletedEventArgs>? Completed;
    event EventHandler<PanelCreatedEventArgs>? PanelCreated;
    event EventHandler<InteractingEventArgs>? Interacting;
}
