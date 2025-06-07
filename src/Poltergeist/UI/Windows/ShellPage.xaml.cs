using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures;
using Poltergeist.Helpers;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;
using Poltergeist.UI.Pages;

namespace Poltergeist.UI.Windows;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    private readonly INavigationService Navigation;
    private readonly MacroManager MacroManager;

    public ShellPage(ShellViewModel viewModel, MacroManager macroManager, INavigationService navigationService, AppEventService eventService)
    {
        ViewModel = viewModel;
        InitializeComponent();

        Navigation = navigationService;
        MacroManager = macroManager;

        navigationService.TabView = NavigationTabView;
        navigationService.NavigationView = NavigationViewControl;

        var startPageKey = App.StartPageKey ?? "home";
        navigationService.NavigateTo(startPageKey);

        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;

        AppTitleBarText.Text = "Poltergeist";

        eventService.Subscribe<AppWindowClosingHandler>(OnAppWindowClosing);
        eventService.Subscribe<MacroCollectionChangedHandler>(OnMacroCollectionChanged);
        eventService.Subscribe<MacroPropertyChangedHandler>(OnMacroPropertyChanged);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
        CreateNavigationMenu();
    }

    private void RefreshHeaderMenu()
    {
        var selectedPageKey = NavigationViewControl.SelectedItem is NavigationViewItem x ? x.Tag as string : null;
        
        NavigationViewControl.MenuItems.Clear();

        var headerItems = Navigation.Informations
            .Where(x => x.PositionInSidebar == NavigationItemPosition.Top)
            ;
        foreach (var item in headerItems)
        {
            var nvi = new NavigationViewItem()
            {
                Content = item.Text ?? item.Header ?? item.Key,
                Tag = item.Key,
                Icon = item.Icon?.ToIconElement(),
                SelectsOnInvoked = item.CreateContent is not null,
            };
            NavigationViewControl.MenuItems.Add(nvi);
        }

        NavigationViewControl.MenuItems.Add(new NavigationViewItemSeparator());
        NavigationViewControl.MenuItems.Add(new NavigationViewItemHeader()
        {
            Content = App.Localize($"Poltergeist/Resources/MacroNavigationViewItemHeader"),
        });

        var sortedShells = MacroManager.Shells
            .Where(x => x.Template is not null)
            //.Where(x => x.Template?.IsSingleton == false)
            .OrderBy(x => x.Properties.IsFavorite)
            .ThenBy(x => x.Title)
            .ToArray();
        foreach (var shell in sortedShells)
        {
            var pageKey = "macro:" + shell.ShellKey;
            var nvi = new NavigationViewItem()
            {
                Content = shell.Title,
                Tag = pageKey,
            };
            if (shell.Icon is not null)
            {
                nvi.Icon = new IconInfo(shell.Icon).ToIconElement();
            }
            nvi.Icon ??= new IconInfo(MacroShell.DefaultIconUri).ToIconElement();
            NavigationViewControl.MenuItems.Add(nvi);
            if ((string)nvi.Tag == selectedPageKey)
            {
                NavigationViewControl.SelectedItem = nvi;
            }
        }
    }

    private void RefreshFooterMenu()
    {
        NavigationViewControl.FooterMenuItems.Clear();

        var footerItems = Navigation.Informations
            .Where(x => x.PositionInSidebar == NavigationItemPosition.Bottom)
            .Reverse()
            ;
        foreach (var item in footerItems)
        {
            var nvi = new NavigationViewItem()
            {
                Content = item.Text ?? item.Header ?? item.Key,
                Tag = item.Key,
                Icon = item.Icon?.ToIconElement(),
                SelectsOnInvoked = item.CreateContent is not null,
            };
            NavigationViewControl.FooterMenuItems.Add(nvi);
        }
    }

    private void CreateNavigationMenu()
    {
        RefreshHeaderMenu();
        RefreshFooterMenu();
    }

    private void OnMacroCollectionChanged(MacroCollectionChangedHandler handler)
    {
        App.TryEnqueue(() =>
        {
            RefreshHeaderMenu();
        });
    }

    private void OnMacroPropertyChanged(MacroPropertyChangedHandler handler)
    {
        App.TryEnqueue(() =>
        {
            var pageKey = "macro:" + handler.Shell.ShellKey;
            var nvi = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => ((string)x.Tag) == pageKey);
            if (nvi is null)
            {
                return;
            }

            nvi.Content = handler.Shell.Title;
            if (handler.Shell.Icon is not null)
            {
                nvi.Icon = new IconInfo(handler.Shell.Icon).ToIconElement();
            }
            else
            {
                nvi.Icon = new IconInfo(MacroShell.DefaultIconUri).ToIconElement();
            }
        });
    }

    private void SelectMenu(string pageKey)
    {
        var menuItems = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().Concat(NavigationViewControl.FooterMenuItems.OfType<NavigationViewItem>());
        var selectedItem = menuItems.FirstOrDefault(x => ((string)x.Tag) == pageKey);
        if (selectedItem is null)
        {
            foreach (var menuItem in menuItems.Where(x => x.MenuItems?.Count > 0))
            {
                selectedItem = menuItem.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => ((string)x.Tag) == pageKey);
                if (selectedItem is not null)
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
        if (args.InvokedItemContainer is not FrameworkElement fe)
        {
            return;
        }

        if (fe.Tag is not string pageKey)
        {
            return;
        }

        Navigation.NavigateTo(pageKey);
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
        if (tvi.Tag is not string pageKey)
        {
            return;
        }

        SelectMenu(pageKey);

        if (tvi.Content is IPageNavigating navigating)
        {
            navigating.OnNavigating();
        }

        if (tvi.Content is IPageNavigated navigated)
        {
            navigated.OnNavigated();
        }
    }

    private void NavigationTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        Navigation.TryCloseTab(args.Tab);
    }

    private void OnAppWindowClosing(AppWindowClosingHandler handler)
    {
        foreach (var tabviewitem in NavigationTabView.TabItems.OfType<TabViewItem>())
        {
            if (tabviewitem.Content is not IApplicationClosing closing)
            {
                continue;
            }

            var canClose = closing.OnApplicationClosing();
            if (!canClose)
            {
                handler.Cancel = true;
            }
        }
    }

}
