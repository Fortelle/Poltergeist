using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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

    private readonly NavigationService Navigation;
    private readonly MacroInstanceManager MacroInstanceManager;

    public ShellPage(ShellViewModel viewModel, MacroInstanceManager macroInstanceManager, NavigationService navigationService, AppEventService eventService)
    {
        ViewModel = viewModel;
        InitializeComponent();

        Navigation = navigationService;
        MacroInstanceManager = macroInstanceManager;

        navigationService.TabView = NavigationTabView;
        navigationService.NavigationView = NavigationViewControl;

        var startPageKey = PoltergeistApplication.Current.StartPageKey ?? "home";
        navigationService.NavigateTo(startPageKey);

        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.Current.MainWindow.ExtendsContentIntoTitleBar = true;
        App.Current.MainWindow.SetTitleBar(AppTitleBar);
        App.Current.MainWindow.Activated += MainWindow_Activated;

        AppTitleBarText.Text = "Poltergeist";

        eventService.Subscribe<AppWindowClosingEvent>(OnAppWindowClosing);
        eventService.Subscribe<MacroInstanceCollectionChangedEvent>(OnMacroInstanceCollectionChanged);
        eventService.Subscribe<MacroInstancePropertyChangedEvent>(OnMacroInstancePropertyChanged);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
        CreateNavigationMenu();

        PoltergeistApplication.GetService<AppEventService>().Publish<AppShellPageLoadedEvent>();
    }

    private void RefreshHeaderMenu()
    {
        var selectedItem = NavigationViewControl.SelectedItem is NavigationViewItem x ? x.Name : null;

        NavigationViewControl.MenuItems.Clear();

        var headerItems = Navigation.SidebarItemInformations
            .Where(x => x.Position == SidebarItemPosition.Top)
            ;
        foreach (var item in headerItems)
        {
            var nvi = new NavigationViewItem()
            {
                Name = item.Navigation?.PageKey,
                Content = item.Text,
                Tag = item.Navigation,
                Icon = IconInfoHelper.ConvertToIconElement(item.Icon),
                SelectsOnInvoked = item.Navigation is not null,
            };
            NavigationViewControl.MenuItems.Add(nvi);
        }

        NavigationViewControl.MenuItems.Add(new NavigationViewItemSeparator());
        NavigationViewControl.MenuItems.Add(new NavigationViewItemHeader()
        {
            Content = App.Localize($"Poltergeist/Resources/MacroNavigationViewItemHeader"),
        });

        var sortedInstances = MacroInstanceManager.GetInstances<MacroInstance>()
            .Where(x => x.Template is not null)
            .OrderBy(x => x.Properties?.IsFavorite == true)
            .ThenBy(x => x.Title)
            .ToArray();
        foreach (var instance in sortedInstances)
        {
            var nvi = new NavigationViewItem()
            {
                Content = instance.Title,
                Tag = new NavigationInfo(instance.GetPageKey()),
                Icon = instance.GetIconElement(),
            };
            NavigationViewControl.MenuItems.Add(nvi);
            if (nvi.Name == selectedItem)
            {
                NavigationViewControl.SelectedItem = nvi;
            }
        }
    }

    private void RefreshFooterMenu()
    {
        NavigationViewControl.FooterMenuItems.Clear();

        var footerItems = Navigation.SidebarItemInformations
            .Where(x => x.Position == SidebarItemPosition.Bottom)
            .Reverse()
            ;
        foreach (var item in footerItems)
        {
            var nvi = new NavigationViewItem()
            {
                Content = item.Text ?? item.Text,
                Tag = (object?)item.Navigation ?? (object?)item.Action,
                Icon = IconInfoHelper.ConvertToIconElement(item.Icon),
                SelectsOnInvoked = item.Navigation is not null,
            };
            NavigationViewControl.FooterMenuItems.Add(nvi);
        }
    }

    private void CreateNavigationMenu()
    {
        RefreshHeaderMenu();
        RefreshFooterMenu();
    }

    private void OnMacroInstanceCollectionChanged(MacroInstanceCollectionChangedEvent _)
    {
        App.TryEnqueue(() =>
        {
            RefreshHeaderMenu();
        });
    }

    private void OnMacroInstancePropertyChanged(MacroInstancePropertyChangedEvent e)
    {
        var instance = e.Instance;

        App.TryEnqueue(() =>
        {
            var pageKey = instance.GetPageKey();
            var nvi = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => x.Name == pageKey);
            if (nvi is null)
            {
                return;
            }

            nvi.Content = instance.Title;
            nvi.Icon = instance.GetIconElement();
        });
    }
    
    private void SelectMenu(string pageKey)
    {
        var menuItems = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().Concat(NavigationViewControl.FooterMenuItems.OfType<NavigationViewItem>());
        var selectedItem = menuItems.FirstOrDefault(x => x.Tag is NavigationInfo ni && ni.PageKey == pageKey);
        if (selectedItem is null)
        {
            foreach (var menuItem in menuItems.Where(x => x.MenuItems?.Count > 0))
            {
                selectedItem = menuItem.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => x.Tag is NavigationInfo ni && ni.PageKey == pageKey);
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
        App.Current.AppTitlebar = AppTitleBarText as UIElement;
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

        if (fe.Tag is NavigationInfo info)
        {
            Navigation.NavigateTo(info.PageKey, info.Argument);
        }
        else if (fe.Tag is Action action)
        {
            action.Invoke();
        }

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

        SelectMenu(tvi.Name);

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

    private void OnAppWindowClosing(AppWindowClosingEvent e)
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
                e.Cancel = true;
            }
        }
    }

}
