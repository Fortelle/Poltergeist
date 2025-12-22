using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class IndicatorInstrumentView : UserControl
{
    public IndicatorInstrumentViewModel ViewModel { get; set; }

    public IndicatorInstrumentView(IndicatorInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
