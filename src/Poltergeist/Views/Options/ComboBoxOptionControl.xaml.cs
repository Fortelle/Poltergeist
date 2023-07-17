using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class ComboBoxOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private object? Value { get; }
    private object[] Choices { get; set; }

    public ComboBoxOptionControl(IOptionItem item)
    {
        InitializeComponent();

        Item = item;
        Value = item.Value;

        switch (item)
        {
            case IChoiceOptionItem choiceoption:
                {
                    Choices = choiceoption.Choices.OfType<object>().ToArray();
                }
                break;
            case { BaseType.IsEnum: true }:
            case IEnumOptionItem:
                {
                    Choices = Enum.GetValues(item.BaseType).OfType<object>().ToArray();
                }
                break;
            default:
                throw new NotSupportedException();
        }
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
