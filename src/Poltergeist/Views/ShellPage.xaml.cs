using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Poltergeist.Services;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;
/// <summary>
/// Interaction logic for ShellPage.xaml
/// </summary>
public partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    public ShellPage(
        ShellViewModel viewModel,
        NavigationService navigationService,
        MacroManager macroManager
        )
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        InitializeComponent();

        foreach (var vNavigationCommand in new RoutedUICommand[]
                {   NavigationCommands.BrowseBack,
                    NavigationCommands.BrowseForward,
                    NavigationCommands.BrowseHome,
                    NavigationCommands.BrowseStop,
                    NavigationCommands.Refresh,
                    NavigationCommands.Favorites,
                    NavigationCommands.Search,
                    NavigationCommands.IncreaseZoom,
                    NavigationCommands.DecreaseZoom,
                    NavigationCommands.Zoom,
                    NavigationCommands.NextPage,
                    NavigationCommands.PreviousPage,
                    NavigationCommands.FirstPage,
                    NavigationCommands.LastPage,
                    NavigationCommands.GoToPage,
                    NavigationCommands.NavigateJournal })
        {
            NavigationFrame.CommandBindings.Add(new CommandBinding(vNavigationCommand, (sender, args) => { }));
        }

        navigationService.Navigate("home");

        foreach (var group in macroManager.Groups)
        {
            NavigationViewControl.MenuItems.Add( new ModernWpf.Controls.NavigationViewItem()
            {
                Content = group.Name,
                Tag = "group_" + group.Name.ToLower(),
                Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Folder)
            });
        }

        viewModel.MenuItems = NavigationViewControl.MenuItems;
    }
    
    public void Ready()
    {

    }

    private void NavigationViewControl_ItemInvoked(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewItemInvokedEventArgs args)
    {
        var tag = args.InvokedItemContainer.Tag as string;
        var nav = App.GetService<NavigationService>();
        nav.Navigate(tag);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
    }

    private ModernWpf.Controls.NavigationViewItem ConvertTemp()
    {
        return HomeMenu;
    }
}
