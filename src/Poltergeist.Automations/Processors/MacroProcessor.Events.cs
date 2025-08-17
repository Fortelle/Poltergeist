using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Automations.Processors;

public sealed partial class MacroProcessor
{
    public event EventHandler<ProcessorLaunchedEventArgs>? Launched;
    public event EventHandler<ProcessorCompletedEventArgs>? Completed;
    public event EventHandler<PanelCreatedEventArgs>? PanelCreated;
    public event EventHandler<InteractingEventArgs>? Interacting;
    public event EventHandler<LogWrittenEventArgs>? LogWritten;

    public void RaiseEvent(ProcessorEvent type, EventArgs eventArgs)
    {
        switch (type)
        {
            case ProcessorEvent.Launched:
                Launched?.Invoke(this, (ProcessorLaunchedEventArgs)eventArgs);
                break;
            case ProcessorEvent.Completed:
                Completed?.Invoke(this, (ProcessorCompletedEventArgs)eventArgs);
                break;
            case ProcessorEvent.PanelCreated:
                PanelCreated?.Invoke(this, (PanelCreatedEventArgs)eventArgs);
                break;
            case ProcessorEvent.Interacting:
                Interacting?.Invoke(this, (InteractingEventArgs)eventArgs);
                break;
            case ProcessorEvent.LogWritten:
                LogWritten?.Invoke(this, (LogWrittenEventArgs)eventArgs);
                break;
            default:
                throw new NotSupportedException();
        }
    }
}
