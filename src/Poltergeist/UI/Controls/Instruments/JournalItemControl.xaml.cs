using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Helpers;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class JournalItemControl : UserControl
{
    public JournalItemControl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not string text)
        {
            throw new InvalidOperationException();
        }

        Content = MarkdownRenderer.RenderLine(text);
    }

}
