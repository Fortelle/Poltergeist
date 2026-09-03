using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Automations.Processors;

public interface IMacroProcessor : IMacroProcessorInformation, IDisposable
{
    void Start();
    ProcessorResult Execute();
    Task<ProcessorResult> ExecuteAsync();
    void Stop(AbortReason reason);
    void Terminate();
    Task Pause(PauseReason reason);
    void Resume();

    void ReceiveMessage(Dictionary<string, string> paramaters);
    bool TryIntervene(string interventionKey);
    bool TryIntervene(ProcessorIntervention intervention);

    event EventHandler<ProcessorLaunchedEventArgs>? Launched;
    event EventHandler<ProcessorCompletedEventArgs>? Completed;
    event EventHandler<PanelCreatedEventArgs>? PanelCreated;
    event EventHandler<InteractingEventArgs>? Interacting;
}
