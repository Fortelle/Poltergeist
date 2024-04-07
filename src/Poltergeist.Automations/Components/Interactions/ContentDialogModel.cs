using Microsoft.UI.Xaml;

namespace Poltergeist.Automations.Components.Interactions;

public class ContentDialogModel : DialogModel
{
    public required FrameworkElement Content { get; init; }

    public Func<string?>? Valid { get; set; }

    public ContentDialogModel()
    {
        Type = DialogType.OkCancel;
    }
}
