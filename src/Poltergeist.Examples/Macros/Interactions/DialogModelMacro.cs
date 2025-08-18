using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class DialogModelMacro : BasicMacro
{
    public DialogModelMacro() : base()
    {
        Title = nameof(DialogModel);

        Category = "Interactions";

        Description = "This macro shows how to pop up dialogs.";

        OptionDefinitions.Add(new ChoiceOption<string>("dialog_type", [
            nameof(DialogType.Ok),
            nameof(DialogType.OkCancel),
            nameof(DialogType.YesNo),
            nameof(DialogType.YesNoCancel),
            "Custom text",
            "With TextBox",
            "With Checkbox",
        ]));

        ExecuteAsync = async (args) =>
        {
            var dialogType = args.Processor.Options.GetValueOrDefault<string>("dialog_type");
            var dialog = dialogType switch
            {
                nameof(DialogType.Ok) => new DialogModel()
                {
                    Text = "This is an OK dialog.",
                    Type = DialogType.Ok,
                },
                nameof(DialogType.OkCancel) => new DialogModel()
                {
                    Text = "This is an OkCancel dialog.",
                    Type = DialogType.OkCancel,
                },
                nameof(DialogType.YesNo) => new DialogModel()
                {
                    Text = "This is a YesNo dialog.",
                    Type = DialogType.YesNo,
                },
                nameof(DialogType.YesNoCancel) => new DialogModel()
                {
                    Text = "This is a YesNoCancel dialog.",
                    Type = DialogType.YesNoCancel,
                },
                "Custom text" => new DialogModel()
                {
                    Text = "This is a three button dialog, with custom button text.",
                    PrimaryButtonText = "Abort",
                    SecondaryButtonText = "Retry",
                    CloseButtonText = "Ignore",
                },
                "With TextBox" => new InputDialogModel()
                {
                    Text = "This is an OkCancel dialog, with an inputbox.",
                    Inputs =
                    [
                        new TextOption("")
                        {
                            Placeholder = "Enter your name",
                        },
                    ],
                },
                "With Checkbox" => new InputDialogModel()
                {
                    Text = "This is an Ok dialog, with a checkbox.",
                    Type = DialogType.Ok,
                    Inputs =
                    [
                        new BoolOption("")
                        {
                            Mode = BoolOptionMode.CheckBox,
                            Text = "Do not show this message again",
                        },
                    ],
                },
                _ => throw new NotImplementedException(),
            };

            var interactionService = args.Processor.GetService<InteractionService>();
            await interactionService.Interact(dialog);
            args.Outputer.Write($"You clicked {dialog.Result}.");
        };
    }
}
