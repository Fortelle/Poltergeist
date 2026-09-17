using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.UI.Pages.Macros;

public sealed partial class InformationView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.RegisterAttached(nameof(ViewModel), typeof(MacroViewModel), typeof(HistoryListView), new PropertyMetadata(null));
    public MacroViewModel? ViewModel
    {
        get => (MacroViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public InformationView()
    {
        InitializeComponent();
    }
}
