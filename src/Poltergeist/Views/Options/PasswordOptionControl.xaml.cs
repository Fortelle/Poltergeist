using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Views.Options;

public sealed partial class PasswordOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private string? Placeholder { get; }

    private int MaxLength { get; }

    private string? Value
    {
        get => Item.Value is PasswordValue password ? password.Value : null;
        set => Item.Value = new PasswordValue(value!);
    }

    public PasswordOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is not PasswordOption passwordOption)
        {
            throw new NotSupportedException();
        }

        Placeholder = passwordOption.Placeholder;
        MaxLength = passwordOption.MaxLength;

        Item = item;

        InitializeComponent();
    }

}
