namespace Poltergeist.Automations.Components.Interactions;

public class DialogModel : InteractionModel
{
    public const string DialogResultKey = "dialog_result";
    public const string DialogParameterKey = "dialog_parameter_{0}";

    public string? Title { get; set; }
    public string? Text { get; set; }
    public DialogType Type { get; set; }

    public bool ShowsDefaultButton { get; set; }
    public string? PrimaryButtonText { get; set; }
    public string? SecondaryButtonText { get; set; }
    public string? CloseButtonText { get; set; }

    public DialogResult Result { get; set; }
    public IReadOnlyDictionary<string, object?>? Parameters { get; set; }
}
