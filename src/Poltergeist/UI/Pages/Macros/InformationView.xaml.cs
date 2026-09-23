using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.UI.Pages.Macros;

[DependencyProperty<MacroViewModel>("ViewModel")]
public sealed partial class InformationView : UserControl
{
    public InformationView()
    {
        InitializeComponent();
    }
}
