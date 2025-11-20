using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class JournalInstrumentView : UserControl
{
    public JournalInstrumentViewModel ViewModel { get; set; }

    public JournalInstrumentView(JournalInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
