using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class DialogModelMacro : CommonOneshotMacroBase
{
    private readonly Dictionary<string, DialogModel> DialogModels = new()
    {
        ["OK"] = new DialogModel()
        {
            Text = "This is an OK dialog.",
            Type = DialogType.Ok,
        },
        ["OK/Cancel"] = new DialogModel()
        {
            Text = "This is an OkCancel dialog.",
            Type = DialogType.OkCancel,
        },
        ["Yes/No"] = new DialogModel()
        {
            Text = "This is a YesNo dialog.",
            Type = DialogType.YesNo,
        },
        ["Yes/No/Cancel"] = new DialogModel()
        {
            Text = "This is a YesNoCancel dialog.",
            Type = DialogType.YesNoCancel,
        },
        ["Custom text"] = new DialogModel()
        {
            Text = "This is a three button dialog, with custom button text.",
            PrimaryButtonText = "Abort",
            SecondaryButtonText = "Retry",
            CloseButtonText = "Ignore",
        },
        ["With TextBox"] = new InputDialogModel()
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
        ["With Checkbox"] = new InputDialogModel()
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
    };

    public DialogModelMacro() : base()
    {
        Title = nameof(DialogModel);

        Category = "Interactions";

        Description = "This macro shows how to pop up dialogs.";

        OptionDefinitions.Add(new ChoiceOption<string>("dialog_type", [.. DialogModels.Keys]));
    }

    protected async override Task OnExecuteAsync(WorkflowController controller)
    {
        var dialogType = controller.Processor.Options.Get<string>("dialog_type");
        var dialogModel = DialogModels[dialogType];

        var interactionService = controller.Processor.GetService<InteractionService>();
        await interactionService.Interact(dialogModel);
        controller.Outputer.Write($"You clicked {dialogModel.Result}.");
    }
}
