using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls.Instruments;

// https://github.com/microsoft/microsoft-ui-xaml/issues/9565
public partial class TextInstrumentView : UserControl
{
    public TextInstrumentViewModel? ViewModel { get; set; }

    public TextInstrumentView(TextInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        viewModel.Bind(TextPanelBox);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (ViewModel is not null)
        {
            ViewModel.Dispose();
        }
    }
}
