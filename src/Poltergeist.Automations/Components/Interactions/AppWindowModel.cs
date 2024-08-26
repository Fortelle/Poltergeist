namespace Poltergeist.Automations.Components.Interactions;

public class AppWindowModel(AppWindowAction action) : InteractionModel
{
    public AppWindowAction Action => action;
}
