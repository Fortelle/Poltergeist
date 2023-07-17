using Microsoft.UI.Xaml.Controls;
using Poltergeist.Macros.Instruments;

namespace Poltergeist.Pages.Macros.Instruments;

public sealed partial class GridInstrumentView : UserControl
{
    public GridInstrumentViewModel ViewModel { get; set; }

    public GridInstrumentView(GridInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        this.InitializeComponent();
    }
}
