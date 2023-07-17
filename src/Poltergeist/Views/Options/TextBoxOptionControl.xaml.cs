using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class TextBoxOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private string? Placeholder { get; }
    private int MaxLenght { get; }

    private string? Value
    {
        get => Item.IsDefault ? null : Item.Value as string;
        set => Item.Value = string.IsNullOrEmpty(value) ? null : value;
    }

    public TextBoxOptionControl(IOptionItem item)
    {
        InitializeComponent();

        Item = item;

        if(item is TextOption textOption)
        {
            Placeholder = textOption.Placeholder;
            MaxLenght = textOption.MaxLenght;
        }
    }

}
