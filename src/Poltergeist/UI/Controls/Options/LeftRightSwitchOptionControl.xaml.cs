using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls.Options;

[ObservableObject]
public sealed partial class LeftRightSwitchOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private string? LeftContent { get; }

    private string? RightContent { get; }

    private bool IsChecked
    {
        get => Item.Value is bool b && b;
        set
        {
            if (value == IsChecked)
            {
                return;
            }

            Item.Value = value;
            OnPropertyChanged(nameof(IsChecked));
        }
    }

    public LeftRightSwitchOptionControl(ObservableParameterItem entry)
    {
        switch (entry.Definition)
        {
            case BoolOption boolOption:
                LeftContent = boolOption.OnText ?? "(left)";
                RightContent = boolOption.OffText ?? "(right)";
                break;
            default:
                throw new NotSupportedException();
        }

        Item = entry;

        InitializeComponent();
    }

    private void LeftTextBlock_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        IsChecked = true;
        e.Handled = true;
    }

    private void RightTextBlock_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        IsChecked = false;
        e.Handled = true;
    }

    private void ToggleFontIcon_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        IsChecked = !IsChecked;
        e.Handled = true;
    }

    private void TextBlock_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        e.Handled = true;
    }
}
