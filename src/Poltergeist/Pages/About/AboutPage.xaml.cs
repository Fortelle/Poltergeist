using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Poltergeist.ViewModels;

namespace Poltergeist.Pages.About;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel
    {
        get;
    }

    public AboutPage(AboutViewModel viewmodel)
    {
        ViewModel = viewmodel;

        this.InitializeComponent();
    }

    private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        //var uri = sender.NavigateUri.ToString();
        //Process.Start(new ProcessStartInfo(uri)
        //{
        //    UseShellExecute = true,
        //    Verb = "open"
        //});
    }
}
