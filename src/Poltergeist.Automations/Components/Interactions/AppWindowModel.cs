namespace Poltergeist.Automations.Components.Interactions;

public class AppWindowModel : InteractionModel
{
    public AppWindowAction Action { get; set; }

    public AppWindowModel(AppWindowAction action)
    {
        Action = action;
    }
}
