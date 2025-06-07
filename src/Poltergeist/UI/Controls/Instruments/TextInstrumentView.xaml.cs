using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls.Instruments;

public partial class TextInstrumentView : UserControl
{
    public TextInstrumentViewModel ViewModel { get; set; }

    public TextInstrumentView(TextInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        viewModel.Bind(TextPanelBox);
    }

}
