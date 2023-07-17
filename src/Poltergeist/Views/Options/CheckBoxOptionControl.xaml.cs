using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class CheckBoxOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private string? OnContent { get; }

    private bool IsChecked
    {
        get => Item.Value is bool b ? b : throw new ArgumentException();
        set => Item.Value = value;
    }

    public CheckBoxOptionControl(IOptionItem item)
    {
        InitializeComponent();

        Item = item;
        if(item is BoolOption boolOption)
        {
            OnContent = boolOption.Text ?? boolOption.OnText;
        }
    }

}
