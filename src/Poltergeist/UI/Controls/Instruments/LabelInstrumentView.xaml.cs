using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class LabelInstrumentView : UserControl
{
    public LabelInstrumentViewModel ViewModel { get; set; }

    public LabelInstrumentView(LabelInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
