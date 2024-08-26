using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Views.Options;

public sealed partial class SwitchOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private string? OnContent { get; }

    private string? OffContent { get; }

    private bool IsChecked
    {
        get => Item.Value is bool b && b;
        set => Item.Value = value;
    }

    public SwitchOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is BoolOption boolOption)
        {
            OnContent = boolOption.OnText ?? null;
            OffContent = boolOption.OffText ?? null;
        }

        Item = item;

        InitializeComponent();
    }

    private void StackPanel_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        ToggleSwitch1.IsOn = !ToggleSwitch1.IsOn;
    }
}
