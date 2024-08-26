using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Interactions;

public class DialogModel : InteractionModel
{
    public const string DialogResultKey = "dialog_result";
    public const string DialogValueKey = "dialog_value_{0}";
    private static readonly string YesButtonText = ResourceHelper.Localize("Poltergeist.Automations/Resources/DialogButton_Yes");
    private static readonly string NoButtonText = ResourceHelper.Localize("Poltergeist.Automations/Resources/DialogButton_No");
    private static readonly string OkButtonText = ResourceHelper.Localize("Poltergeist.Automations/Resources/DialogButton_Ok");
    private static readonly string CancelButtonText = ResourceHelper.Localize("Poltergeist.Automations/Resources/DialogButton_Cancel");

    public string? Title { get; set; }
    public string? Text { get; set; }
    public DialogType Type { get; set; }

    public bool ShowsDefaultButton { get; set; }
    public string? PrimaryButtonText { get; set; }
    public string? SecondaryButtonText { get; set; }
    public string? CloseButtonText { get; set; }

    public DialogResult Result { get; set; }
    public object?[]? Values { get; set; }

    public override void Callback(InteractionCallbackArguments args)
    {
        args.AllowsResume = true;
    }

    public (string? Primary, string? Secondary, string? Close) GetButtonNames()
    {
        return Type switch
        {
            DialogType.None => (null, null, OkButtonText),
            DialogType.Ok => (null, null, OkButtonText),
            DialogType.OkCancel => (OkButtonText, null, CancelButtonText),
            DialogType.YesNo => (YesButtonText, null, NoButtonText),
            DialogType.YesNoCancel => (YesButtonText, NoButtonText, CancelButtonText),
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
