using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public BasicMacro DialogTestMacro = new("test_dialog")
    {
        Title = "Dialog Test",

        Description = "This macro is used for testing the dialog service.",

        UserOptions =
        {
            new ChoiceOption<string>("dialog_type", new[]
            {
                "Ok",
                "OkCancel",
                "YesNo",
                "YesNoCancel",
                "AbortRetryIgnore",
                "Input",
                "Checkbox",
            }, "Ok")
        },

        AsyncExecution = async (args) =>
        {
            var interactionService = args.Processor.GetService<InteractionService>();
            var dialogType = args.Processor.Options.Get("dialog_type", "");
            var dialog = dialogType switch
            {
                "Ok" => new DialogModel()
                {
                    Text = "This is an OK dialog.",
                    Type = DialogType.Ok,
                },
                "OkCancel" => new DialogModel()
                {
                    Text = "This is an OkCancel dialog.",
                    Type = DialogType.OkCancel,
                },
                "YesNo" => new DialogModel()
                {
                    Text = "This is a YesNo dialog.",
                    Type = DialogType.YesNo,
                },
                "YesNoCancel" => new DialogModel()
                {
                    Text = "This is a YesNoCancel dialog.",
                    Type = DialogType.YesNoCancel,
                },
                "AbortRetryIgnore" => new DialogModel()
                {
                    Text = "This is a three button dialog, with custom button text.",
                    PrimaryButtonText = "Abort",
                    SecondaryButtonText = "Retry",
                    CloseButtonText = "Ignore",
                },
                "Input" => new DialogModel()
                {
                    Text = "This is an OkCancel dialog, with an inputbox.",
                    Type = DialogType.OkCancel,
                    Inputs = new IOptionItem[] {
                    new TextOption("")
                    {
                        Placeholder = "Enter your name",
                    },
                },
                },
                "Checkbox" => new DialogModel()
                {
                    Text = "This is an Ok dialog, with a checkbox.",
                    Type = DialogType.Ok,
                    Inputs = new IOptionItem[] {
                    new BoolOption("")
                    {
                        Mode = BoolOptionMode.CheckBox,
                        Text = "Do not show this message again",
                    },
                },
                },
                _ => throw new NotImplementedException(),
            };

            await interactionService.ShowAsync(dialog);
            args.Outputer.Write($"You clicked {dialog.Result}.");
        }

    };

}
