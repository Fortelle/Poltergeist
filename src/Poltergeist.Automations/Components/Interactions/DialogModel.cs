using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Automations.Components.Interactions;

public class DialogModel : InteractionModel
{
    public const string DialogResultKey = "dialog_result";
    public const string DialogValueKey = "dialog_value_{0}";

    public string? Title { get; set; }
    public string? Text { get; set; }
    public IOptionItem[]? Inputs { get; set; }
    public DialogType Type { get; set; }

    public bool ShowsDefaultButton { get; set; }
    public string? PrimaryButtonText { get; set; }
    public string? SecondaryButtonText { get; set; }
    public string? CloseButtonText { get; set; }

    public DialogResult Result { get; set; }
    public object[]? Values { get; set; }

    public override void Callback(InteractionCallbackArguments args)
    {
        args.AllowsResume = true;
    }

    public (string? Primary, string? Secondary, string? Close) GetButtonNames()
    {
        return Type switch
        {
            DialogType.None => (null, null, "OK"),
            DialogType.Ok => (null, null, "OK"),
            DialogType.OkCancel => ("OK", null, "Cancel"),
            DialogType.YesNo => ("Yes", null, "No"),
            DialogType.YesNoCancel => ("Yes", "No", "Cancel"),
            _ => (null, null, null),
        };
    }

    public DialogResult GetDialogResult(ContentDialogResult result)
    {
        return (Type, result) switch
        {
            (DialogType.None, ContentDialogResult.None) => DialogResult.Close,
            (DialogType.None, ContentDialogResult.Secondary) => DialogResult.Secondary,
            (DialogType.None, ContentDialogResult.Primary) => DialogResult.Primary,

            (DialogType.Ok, ContentDialogResult.None) => DialogResult.Ok,

            (DialogType.OkCancel, ContentDialogResult.Primary) => DialogResult.Ok,
            (DialogType.OkCancel, ContentDialogResult.None) => DialogResult.Cancel,

            (DialogType.YesNo, ContentDialogResult.Primary) => DialogResult.Yes,
            (DialogType.YesNo, ContentDialogResult.None) => DialogResult.No,

            (DialogType.YesNoCancel, ContentDialogResult.Primary) => DialogResult.Yes,
            (DialogType.YesNoCancel, ContentDialogResult.Secondary) => DialogResult.No,
            (DialogType.YesNoCancel, ContentDialogResult.None) => DialogResult.Cancel,

            _ => DialogResult.Unknown,
        };
    }
}
