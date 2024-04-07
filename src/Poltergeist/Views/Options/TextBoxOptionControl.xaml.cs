using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class TextBoxOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }
    private string? Placeholder { get; }
    private int MaxLenght { get; }

    private string? Value
    {
        get => Item.Value as string;
        set
        {
            Item.Value = string.IsNullOrEmpty(value) ? null : value;

            if (Item.Definition is TextOption textOption && textOption.Valid is not null)
            {
                HasError = !textOption.IsValid(value);
            }
        }
    }

    [ObservableProperty]
    private bool _hasError;

    public TextBoxOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is TextOption textOption)
        {
            Placeholder = textOption.Placeholder;
            MaxLenght = textOption.MaxLenght;
            HasError = !textOption.IsValid(item.Value as string);
        }

        Item = item;

        InitializeComponent();
    }

}
