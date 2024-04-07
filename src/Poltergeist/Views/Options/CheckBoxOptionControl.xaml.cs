using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Views.Options;

public sealed partial class CheckBoxOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }
    private string? OnContent { get; }

    private bool IsChecked
    {
        get => Item.Value is bool b ? b : false;
        set => Item.Value = value;
    }

    public CheckBoxOptionControl(ObservableParameterItem item)
    {
        switch (item.Definition)
        {
            case BoolOption boolOption:
                OnContent = boolOption.Text ?? boolOption.OnText;
                break;
            default:
                throw new NotSupportedException();
        }

        Item = item;

        InitializeComponent();
    }
}
