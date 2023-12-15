using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class ComboBoxOptionControl : UserControl
{
    public static readonly DependencyProperty ItemProperty = DependencyProperty.RegisterAttached("Item", typeof(IOptionItem), typeof(ComboBoxOptionControl), new PropertyMetadata(null));

    private ChoiceEntry[]? Choices { get; set; }

    public IOptionItem? Item
    {
        get => (IOptionItem?)GetValue(ItemProperty);
        set
        {
            SetValue(ItemProperty, value);

            if (value is null)
            {
                return;
            }

            Choices = value switch
            {
                IChoiceOptionItem choiceoption => choiceoption.GetChoices(),
                { BaseType.IsEnum: true } => Enum.GetValues(value.BaseType).OfType<object>().Select(x => new ChoiceEntry(x)).ToArray(),
                _ => throw new NotSupportedException(),
            };
        }
    }

    private object? SelectedValue
    {
        get => Choices?.FirstOrDefault(x => x.Value?.Equals(Item?.Value) ?? false)?.Value;
        set
        {
            if (Item is null)
            {
                return;
            }

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

    public ComboBoxOptionControl()
    {
        InitializeComponent();
    }

    public ComboBoxOptionControl(IOptionItem item) : this()
    {
        Item = item;
    }

}
