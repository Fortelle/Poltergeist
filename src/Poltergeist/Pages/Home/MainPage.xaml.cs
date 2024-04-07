using Microsoft.UI.Xaml.Controls;
using Poltergeist.Pages;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class MainPage : Page, IPageNavigating
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    public void OnNavigating()
    {
    }
}
