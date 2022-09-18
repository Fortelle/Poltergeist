using System.Windows;
using System.Windows.Controls;
using Poltergeist.Automations.Macros;
using Poltergeist.Services;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    private void StackPanel_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is MacroBase macro)
        {
            var manager = App.GetService<MacroManager>();
            manager.Set(macro);
        }
    }

}
