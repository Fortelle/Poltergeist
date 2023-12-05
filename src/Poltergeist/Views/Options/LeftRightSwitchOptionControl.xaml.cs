using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class LeftRightSwitchOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private string LeftContent { get; }
    private string RightContent { get; }

    private bool IsChecked
    {
        get => (bool)Item.Value!;
        set
        {
            if(value == IsChecked)
            {
                return;
            }

            Item.Value = value;
            OnPropertyChanged(nameof(IsChecked));
        }
    }

    public LeftRightSwitchOptionControl(IOptionItem item)
    {
        InitializeComponent();

        Item = item;
        if(item is BoolOption boolOption)
        {
            LeftContent = boolOption.OnText ?? "£¨left)";
            RightContent = boolOption.OffText ?? "(right)";
        }
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
