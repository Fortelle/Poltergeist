using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class PasswordOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private string? Placeholder { get; }
    private int MaxLenght { get; }

    private string? Value
    {
        get => ((PasswordValue)Item.Value).Value;
        set => Item.Value = new PasswordValue(value);
    }

    public PasswordOptionControl(PasswordOption passwordOption)
    {
        InitializeComponent();

        Item = passwordOption;

        Placeholder = passwordOption.Placeholder;
        MaxLenght = passwordOption.MaxLenght;
    }

}
