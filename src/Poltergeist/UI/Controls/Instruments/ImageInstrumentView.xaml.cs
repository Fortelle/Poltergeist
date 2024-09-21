using Microsoft.UI.Xaml.Controls;
using Poltergeist.UI.Controls.Instruments;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class ImageInstrumentView : UserControl
{
    public ImageInstrumentViewModel ViewModel { get; set; }

    public ImageInstrumentView(ImageInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
