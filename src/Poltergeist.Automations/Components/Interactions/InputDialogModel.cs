using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Components.Interactions;

public class InputDialogModel : DialogModel
{
    public required IOptionDefinition[] Inputs { get; init; }

    public InputDialogLabelLayout LabelLayout { get; set; }

    public Func<IReadOnlyDictionary<string, object?>?, string?>? Valid { get; set; }

    public InputDialogModel()
    {
        Type = DialogType.OkCancel;
    }
}
