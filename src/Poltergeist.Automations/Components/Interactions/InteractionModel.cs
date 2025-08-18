namespace Poltergeist.Automations.Components.Interactions;

public abstract class InteractionModel : NotificationModel
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public TimeSpan Timeout { get; set; }
}
