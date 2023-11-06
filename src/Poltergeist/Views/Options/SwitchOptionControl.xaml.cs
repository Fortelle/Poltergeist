using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;

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
        OnContent = "";
        OffContent = "";
    }
}