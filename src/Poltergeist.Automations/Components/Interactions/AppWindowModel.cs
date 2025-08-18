namespace Poltergeist.Automations.Components.Interactions;

public class AppWindowModel(AppWindowAction action) : NotificationModel
{
    public AppWindowAction Action => action;
}
