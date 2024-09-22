using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Automations.Processors;

public sealed partial class MacroProcessor
{
    public event EventHandler<ProcessorLaunchedEventArgs>? Launched;
    public event EventHandler<ProcessorCompletedEventArgs>? Completed;
    public event EventHandler<PanelCreatedEventArgs>? PanelCreated;
    public event EventHandler<InteractingEventArgs>? Interacting;

    public void RaiseEvent(ProcessorEvent type, EventArgs eventArgs)
    {
        MulticastDelegate? multicastDelegate = type switch
        {
            ProcessorEvent.Launched => Launched,
            ProcessorEvent.Completed => Completed,
            ProcessorEvent.PanelCreated => PanelCreated,
            ProcessorEvent.Interacting => Interacting,
            _ => throw new NotSupportedException(),
        };
        if (multicastDelegate is null)
        {
            return;
        }

        var handlerArgs = new object[] { this, eventArgs };

        OriginalContext!.Post(d =>
        {
            multicastDelegate?.DynamicInvoke(handlerArgs);
        }, null);
    }

    public void RaiseAction(Action action)
    {
        OriginalContext!.Post(d =>
        {
            action.DynamicInvoke();
        }, null);
    }
}
