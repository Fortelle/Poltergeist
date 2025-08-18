using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls.Options;

[ObservableObject]
public sealed partial class TextBoxOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private string? Placeholder { get; }

    private int MaxLength { get; }

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
    public partial bool HasError { get; set; }

    public TextBoxOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is TextOption textOption)
        {
            Placeholder = textOption.Placeholder;
            MaxLength = textOption.MaxLength;
            HasError = !textOption.IsValid(item.Value as string);
        }

        Item = item;

        InitializeComponent();
    }

}
