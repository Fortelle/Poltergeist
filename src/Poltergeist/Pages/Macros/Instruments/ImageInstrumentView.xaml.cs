using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.Pages.Macros.Instruments;

public sealed partial class ImageInstrumentView : UserControl
{
    public ImageInstrumentViewModel ViewModel
    {
        get; set;
    }

    public ImageInstrumentView(ImageInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        this.InitializeComponent();
    }
}
