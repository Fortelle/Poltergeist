using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class TileInstrumentView : UserControl
{
    public TileInstrumentViewModel ViewModel { get; set; }

    public TileInstrumentView(TileInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
