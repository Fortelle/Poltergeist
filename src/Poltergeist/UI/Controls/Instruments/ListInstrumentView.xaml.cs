using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Modules.Macros;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class ListInstrumentView : UserControl
{
    public ListInstrumentViewModel ViewModel { get; set; }

    public ListInstrumentView(ListInstrumentViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    private void HyperlinkButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var button = (HyperlinkButton)sender;
        var argument = (string)button.Tag;
        var msg = new InteractionMessage(argument);
        App.GetService<MacroManager>().SendMessage(msg);
    }

}
