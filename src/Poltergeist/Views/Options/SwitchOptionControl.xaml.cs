using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class SwitchOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private string? OnContent { get; }
    private string? OffContent { get; }

    private bool IsChecked
    {
        get => (bool)Item.Value!;
        set => Item.Value = value;
    }

    public SwitchOptionControl(IOptionItem item)
    {
        InitializeComponent();

        Item = item;

        if (item is BoolOption boolOption)
        {
            OnContent = boolOption.OnText ?? null;
            OffContent = boolOption.OffText ?? null;
        }
    }

    private void StackPanel_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        ToggleSwitch1.IsOn = !ToggleSwitch1.IsOn;
    }
}
