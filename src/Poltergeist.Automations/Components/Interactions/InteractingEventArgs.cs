namespace Poltergeist.Automations.Components.Interactions;

public class InteractingEventArgs : EventArgs
{
    public InteractionModel Model { get; set; }
    public bool Suspended { get; set; }

    public InteractingEventArgs(InteractionModel model)
    {
        Model = model;
        Suspended = false;
    }

    public InteractingEventArgs(InteractionModel model, bool suspended)
    {
        Model = model;
        Suspended = suspended;
    }
}
