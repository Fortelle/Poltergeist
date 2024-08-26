namespace Poltergeist.Automations.Components.Interactions;

// todo: buttons
public class ToastModel : InteractionModel
{
    public string? Title { get; set; }
    public string? Text { get; set; }
    //public IOptionItem[]? Inputs { get; set; }

    public Uri? ImageUri { get; set; }
}
