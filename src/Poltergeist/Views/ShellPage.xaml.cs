using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Macros;
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

    public ShellPage(ShellViewModel viewModel, INavigationService navigationService)
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
            App.ContentLoaded += () =>
            {
                var macroManager = App.GetService<MacroManager>();
                macroManager.MacroCollectionChanged += CreateNavigationMenu;
                macroManager.MacroPropertyChanged += UpdateNavigationMenu;
            };
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
        CreateNavigationMenu();
    }

    private void CreateNavigationMenu()
    {
        var list = new List<NavigationViewItemBase>();
        var headerIndex = NavigationViewControl.MenuItems.IndexOf(MacroNavigationViewItemHeader);
        list.AddRange(NavigationViewControl.MenuItems.Take(headerIndex + 1).Cast<NavigationViewItemBase>());

        var macroManager = App.GetService<MacroManager>();
        var navigationService = App.GetService<INavigationService>();
        var selectedPageKey = NavigationViewControl.SelectedItem is NavigationViewItem x ? x.Tag as string : null;
        var macros = macroManager.Shells
            .Where(x => x.Template is not null)
            .Where(x => x.Template?.IsSingleton == false)
            .OrderBy(x => x.Properties.IsFavorite)
            .ThenBy(x => x.Title)
            ;

        foreach (var shell in macros)
        {
            var navInfo = navigationService.GetInfo("macro")!;
            var pageKey = "macro:" + shell.ShellKey;
            var nvi = new NavigationViewItem()
            {
                Content = shell.Title,
                Tag = pageKey,
                IsSelected = pageKey == selectedPageKey,
            };
            if (shell.Icon is not null)
            {
                nvi.Icon = new IconInfo(shell.Icon).ToIconElement();
            }
            if (nvi.Icon is null)
            {
                nvi.Icon = new IconInfo(MacroShell.DefaultIconUri).ToIconElement();
            }
            list.Add(nvi);
        }

        NavigationViewControl.MenuItems.Clear();
        foreach (var item in list)
        {
            NavigationViewControl.MenuItems.Add(item);
        }
        if (selectedPageKey is not null)
        {
            SelectMenu(selectedPageKey);
        }
    }

    private void UpdateNavigationMenu()
    {
        var macroManager = App.GetService<MacroManager>();

        var headerIndex = NavigationViewControl.MenuItems.IndexOf(MacroNavigationViewItemHeader);
        var items = NavigationViewControl.MenuItems
            .Skip(headerIndex + 1)
            .OfType<NavigationViewItem>()
            .Where(x => x.Tag is string s && s.StartsWith("macro:"))
            ;
        foreach (var nvi in items)
        {
            var shellKey = ((string)nvi.Tag).Split(':', 2)[1];
            var shell = macroManager.GetShell(shellKey)!;
            nvi.Content = shell.Title;
            if (shell.Icon is not null)
            {
                nvi.Icon = new IconInfo(shell.Icon).ToIconElement();
            }
            if (nvi.Icon is null)
            {
                nvi.Icon = new IconInfo(MacroShell.DefaultIconUri).ToIconElement();
            }
        }
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
        App.GetService<INavigationService>().TryCloseTab(args.Tab);
    }

    private void DebugNavigationViewItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        Debug.Do();
    }
}
