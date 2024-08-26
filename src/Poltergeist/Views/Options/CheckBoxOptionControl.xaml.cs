using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Views.Options;

public sealed partial class CheckBoxOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private string? OnContent { get; }

    private bool IsChecked
    {
        get => Item.Value is bool b && b;
        set => Item.Value = value;
    }

    public CheckBoxOptionControl(ObservableParameterItem item)
    {
        OnContent = item.Definition switch
        {
            BoolOption boolOption => boolOption.Text ?? boolOption.OnText,
            _ => throw new NotSupportedException(),
        };
        Item = item;

        InitializeComponent();
    }
}
