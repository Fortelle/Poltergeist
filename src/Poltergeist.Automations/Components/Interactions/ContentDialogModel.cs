namespace Poltergeist.Automations.Components.Interactions;

public class ContentDialogModel : DialogModel
{
    public required object Content { get; init; }

    public Func<string?>? Valid { get; set; }

    public ContentDialogModel()
    {
        Type = DialogType.OkCancel;
    }
}
