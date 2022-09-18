using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel { get; }

    public AboutPage(AboutViewModel viewmodel)
    {
        ViewModel = viewmodel;
        InitializeComponent();
    }

    private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var hl = (Hyperlink)sender;
        var uri = hl.Tag.ToString();
        Process.Start(new ProcessStartInfo(uri)
        {
            UseShellExecute = true,
            Verb = "open"
        });
        e.Handled = true;
    }
}
