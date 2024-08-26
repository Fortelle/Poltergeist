using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Views.Options;

public sealed partial class ComboBoxOptionControl : UserControl
{
    private ObservableParameterItem Item { get; set; }

    private ChoiceEntry[]? Choices { get; set; }

    private object? SelectedValue
    {
        get => Choices?.FirstOrDefault(x => x.Value?.Equals(Item.Value) ?? false)?.Value;
        set
        {
            if (value is ChoiceEntry entry)
            {
                Item.Value = entry.Value;
            }
            else
            {
                Item.Value = value;
            }
        }
    }

    private static ChoiceEntry[] GetEnumChoices(Type type)
    {
        return Enum.GetValues(type).Cast<object>().Select(x => new ChoiceEntry(x)).ToArray();
    }

    public ComboBoxOptionControl(ObservableParameterItem item)
    {
        Choices = item.Definition switch
        {
            IChoiceOption choiceoption => choiceoption.GetChoices(),
            { BaseType.IsEnum: true } => GetEnumChoices(item.Definition.BaseType),
            _ => throw new NotSupportedException(),
        };

        Item = item;

        InitializeComponent();
    }

}
