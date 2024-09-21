using Microsoft.UI.Xaml.Controls;
using Poltergeist.UI.Pages.About;

namespace Poltergeist.UI.Pages.About;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel { get; }

    public AboutPage(AboutViewModel viewmodel)
    {
        ViewModel = viewmodel;

        InitializeComponent();
    }

}
