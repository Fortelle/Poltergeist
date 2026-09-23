using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Macros;

namespace Poltergeist.UI.Controls.Options;

[DependencyProperty<ChoiceEntry[]>("Choices")]
public sealed partial class ComboBoxOptionControl : UserControl
{
    private ObservableParameterItem Item { get; set; }

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
            { ValueType.IsEnum: true } => GetEnumChoices(item.Definition.ValueType),
            IMacroChoiceOption => null,
            IDynamicChoiceOption => null,
            _ => throw new NotSupportedException(),
        };

        Item = item;

        InitializeComponent();
    }

    private void ComboBox_DropDownOpened(object sender, object e)
    {
        if (Item.Definition is IMacroChoiceOption mco)
        {
            Choices = App.GetService<MacroInstanceManager>()
                .GetInstances()
                .Where(x => x.IsValid && mco.Predicate(x.Template))
                .Select(x => new ChoiceEntry(x.InstanceId, x.Title))
                .ToArray();
        }
        else if (Item.Definition is IDynamicChoiceOption dco)
        {
            Choices = dco.GetChoices();
        }
    }
}
