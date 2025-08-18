namespace Poltergeist.Automations.Components.Interactions;

public class InteractingEventArgs : EventArgs
{
    public NotificationModel Model { get; set; }

    public InteractingEventArgs(NotificationModel model)
    {
        Model = model;
    }

}
