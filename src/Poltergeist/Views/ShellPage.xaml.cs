using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Contracts.Services;
using Poltergeist.Helpers;
using Poltergeist.Pages;
using Poltergeist.Services;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel, MacroManager macroManager, INavigationService navigationService)
    {
        ViewModel = viewModel;
        InitializeComponent();

        navigationService.TabView = NavigationTabView;
        navigationService.NavigationView = NavigationViewControl;

        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;

        AppTitleBarText.Text = "Poltergeist";

        if (App.SingleMacroMode is null)
        {
            foreach (var group in macroManager.Groups)
            {
                var groupInfo = navigationService.GetInfo("group")!;
                var nvi = new NavigationViewItem()
                {
                    Content = $"{group.Key} ({group.Macros.Count})",
                    Tag = "group:" + group.Key,
                    //IsExpanded = true,
                    Icon = new FontIcon()
                    {
                        Glyph = groupInfo.Glyph,
                    }
                };

                foreach (var macro in group.Macros.OrderBy(x => x.Title))
                {
                    var macroInfo = navigationService.GetInfo("macro")!;
                    nvi.MenuItems.Add(new NavigationViewItem()
                    {
                        Content = macro.Title,
                        Tag = "macro:" + macro.Key,
                        Icon = new FontIcon()
                        {
                            Glyph = macroInfo.Glyph,
                        }
                    });
                }
                NavigationViewControl.MenuItems.Add(nvi);
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = AppTitleBar.Margin with
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
        };
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is not FrameworkElement fe) return;
        if (fe.Tag is not string value) return;

        App.GetService<INavigationService>().NavigateTo(value);
    }

    private void NavigationTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!e.AddedItems.Any()) // close tab
        {
            return;
        }
        if (e.AddedItems[0] is not TabViewItem tvi)
        {
            return;
        }
        if (tvi.Tag is not string tabKey)
        {
            return;
        }

        var menuItems = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().Concat(NavigationViewControl.FooterMenuItems.OfType<NavigationViewItem>()) ;
        var selectedItem = menuItems.FirstOrDefault(x => ((string)x.Tag) == tabKey);
        if(selectedItem is null)
        {
            foreach(var menuItem in menuItems.Where(x => x.MenuItems?.Count > 0))
            {
                selectedItem = menuItem.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => ((string)x.Tag) == tabKey);
                if(selectedItem is not null)
                {
                    if (NavigationViewControl.IsPaneOpen)
                    {
                        menuItem.IsExpanded = true;
                    }
                    break;
                }
            }
        }

        NavigationViewControl.SelectedItem = selectedItem;

        if (tvi.Content is IPageNavigating navigating)
        {
            navigating.OnNavigating();
        }
    }

    private void NavigationTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if(args.Tab.Content is IPageClosing pageclosing)
        {
            if (!pageclosing.OnPageClosing())
            {
                return;
            }
        }

        sender.TabItems.Remove(args.Tab);
    }

    private void NavigationViewItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
    }

    private void DebugNavigationViewItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        Debug.Do();
    }
}
