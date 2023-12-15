using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class TextBoxOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private string? Placeholder { get; }
    private int MaxLenght { get; }

    private string? Value
    {
        get => Item.Value as string;
        set
        {
            Item.Value = string.IsNullOrEmpty(value) ? null : value;

            if (Item is TextOption textOption && textOption.Valid is not null)
            {
                HasError = !textOption.IsValid;
            }
        }
    }

    [ObservableProperty]
    private bool _hasError;

    public TextBoxOptionControl(IOptionItem item)
    {
        InitializeComponent();

        Item = item;

        if(item is TextOption textOption)
        {
            Placeholder = textOption.Placeholder;
            MaxLenght = textOption.MaxLenght;
            HasError = !textOption.IsValid;
        }
    }

}
