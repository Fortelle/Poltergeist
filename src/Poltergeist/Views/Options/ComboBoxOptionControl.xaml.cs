using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class ComboBoxOptionControl : UserControl
{
    public static readonly DependencyProperty ItemProperty = DependencyProperty.RegisterAttached("Item", typeof(IOptionItem), typeof(ComboBoxOptionControl), new PropertyMetadata(null));

    private object? Value { get; set; }
    private object[]? Choices { get; set; }

    public IOptionItem? Item
    {
        get => (IOptionItem?)GetValue(ItemProperty);
        set
        {
            SetValue(ItemProperty, value);
            if (value is null) return;

            Value = value.Value;
            Choices = value switch
            {
                IChoiceOptionItem choiceoption => choiceoption.Choices.OfType<object>().ToArray(),
                { BaseType.IsEnum: true } or IEnumOptionItem => Enum.GetValues(value.BaseType).OfType<object>().ToArray(),
                _ => throw new NotSupportedException(),
            };
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

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var value = e.AddedItems[0];
        if (Item.Value?.ToString() == value.ToString())
        {
            return;
        }

        Item.Value = value;
    }
}
