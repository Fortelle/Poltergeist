using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.Pages.Macros.Instruments;

public sealed partial class GridInstrumentView : UserControl
{
    public GridInstrumentViewModel ViewModel { get; set; }

    public GridInstrumentView(GridInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
