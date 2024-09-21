using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Pages.Home;

public sealed partial class MainPage : Page, IPageClosed
{
    public MainViewModel ViewModel { get; }

    public MainPage(MainViewModel viewmodel)
    {
        ViewModel = viewmodel;
        InitializeComponent();
    }

    public void OnPageClosed()
    {
        MacroBrowser1.ViewModel.Dispose();
    }
}
