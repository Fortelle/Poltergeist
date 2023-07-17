using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.Macros.Instruments;

public partial class TextInstrumentView : UserControl
{
    public TextInstrumentViewModel ViewModel
    {
        get; set;
    }

    public TextInstrumentView(TextInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        viewModel.RichTextBlock = TextPanelBox;
        viewModel.Refresh();
    }

}
