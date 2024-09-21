using Microsoft.UI.Xaml.Controls;
using Poltergeist.UI.Controls.Instruments;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class GridInstrumentView : UserControl
{
    public GridInstrumentViewModel ViewModel { get; set; }

    public GridInstrumentView(GridInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
