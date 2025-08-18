using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Helpers;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Macros;
using Poltergeist.UI.Pages;

namespace Poltergeist.Modules.Navigation;

public class NavigationService : ServiceBase
{
    public List<SidebarItemInfo> SidebarItemInformations { get; } = new();
    public List<PageInfo> PageInformations { get; } = new();

    public TabView? TabView { get; set; }

    public NavigationView? NavigationView { get; set; }

    public NavigationService(AppEventService eventService)
    {
        eventService.Subscribe<AppWindowClosedEvent>(OnAppWindowClosed);
    }

    public void AddSidebarItemInfo(SidebarItemInfo info)
    {
        SidebarItemInformations.Add(info);
    }

    public void AddPageInfo(PageInfo info)
    {
        PageInformations.Add(info);
    }

    public bool NavigateTo(string pageKey, object? data = null)
    {
        if (TabView is null)
        {
            return false;
        }

        var info = GetPageInfo(pageKey);
        if (info is null)
        {
            Logger.Error($"Failed to navigate: Invalid page key '{pageKey}'.");
            return false;
        }

        if (!CanCreateTab(pageKey))
        {
            Logger.Error($"Could not switch to tab page '{pageKey}' in exclusive macro mode.");
            return false;
        }

        if (!TryGetTab(pageKey, out var tab))
        {
            tab = CreateTab(pageKey, info, data);
        }
        else if (info.UpdateArgument is not null)
        {
            var page = (Page)tab.Content;
            info.UpdateArgument.Invoke(page, data);
        }

        if (TabView.SelectedItem is not TabViewItem tvi || tvi != tab)
        {
            TabView.SelectedItem = tab;
        }

        Logger.Trace($"Navigated to tab page '{pageKey}'.");

        return true;
    }

    public bool NavigateTo(NavigationInfo info)
    {
        return NavigateTo(info.PageKey, info.Argument);
    }

    public bool NavigateTo(PageInfo info, object? data = null)
    {
        var pageKey = info.Key;

        if (!TryGetTab(pageKey, out var tab))
        {
            tab = CreateTab(pageKey, info, data);
        }
        else if (info.UpdateArgument is not null)
        {
            var page = (Page)tab.Content;
            info.UpdateArgument.Invoke(page, data);
        }

        if (TabView?.SelectedItem is not TabViewItem tvi || tvi != tab)
        {
            TabView?.SelectedItem = tab;
        }

        Logger.Trace($"Navigated to tab page '{pageKey}'.");

        return true;
    }

    private TabViewItem? CreateTab(string pageKey, PageInfo info, object? data = null)
    {
        try
        {
            var content = info.CreateContent!.Invoke(pageKey, data);
            var header = info.CreateHeader?.Invoke(content) ?? info.Header ?? info.Key;
            var icon = info.CreateIcon?.Invoke(content) ?? IconInfoHelper.ConvertToIconSource(info.Icon);
            var menuItems = info.CreateMenu?.Invoke(content) ?? info.Menu;

            var tab = new TabViewItem
            {
                Name = pageKey,
                Header = header,
                Content = content,
                IconSource = icon,
            };

            tab.RightTapped += (_, e) =>
            {
                tab.ContextFlyout ??= CreateTabPageContextFlyout(menuItems, pageKey);
            };

            if (pageKey == "home")
            {
                TabView!.TabItems.Insert(0, tab);
            }
            else
            {
                TabView!.TabItems.Add(tab);
            }

            Logger.Trace($"Created tab page '{pageKey}'.");

            PoltergeistApplication.GetService<AppEventService>().Publish(new AppWindowPageCreatedEvent(pageKey));

            return tab;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to create tab page: {ex.Message}");
            PoltergeistApplication.ShowException(ex);
            return null;
        }
    }

    public bool TryGetTab(string pageKey, [MaybeNullWhen(false)] out TabViewItem tab)
    {
        if (TabView is null)
        {
            tab = null;
            return false;
        }

        tab = TabView.TabItems.OfType<TabViewItem>().FirstOrDefault(x => x.Name == pageKey);
        return tab is not null;
    }

    public bool TryCloseTab(string pageKey)
    {
        if (TabView is null)
        {
            return true;
        }

        if (!TryGetTab(pageKey, out var tab))
        {
            return true;
        }

        return TryCloseTab(tab);
    }

    public bool TryCloseTab(TabViewItem tab)
    {
        if (TabView is null)
        {
            return true;
        }

        if (tab.Content is IPageClosing pageclosing)
        {
            var canClose = pageclosing.OnPageClosing();
            if (!canClose)
            {
                return false;
            }
        }

        TabView.TabItems.Remove(tab);

        if (tab.Content is IPageClosed pageclosed)
        {
            pageclosed.OnPageClosed();
        }

        Logger.Trace($"Removed tab page '{tab.Name}'.");

        var pageKey = tab.Name;
        PoltergeistApplication.GetService<AppEventService>().Publish(new AppWindowPageClosedEvent(pageKey));

        return true;
    }

    private bool CanCreateTab(string pageKey)
    {
        var exclusiveMacroMode = PoltergeistApplication.Current.ExclusiveMacroMode;
        if (!string.IsNullOrEmpty(exclusiveMacroMode) && pageKey != MacroManager.GetPageKey(exclusiveMacroMode))
        {
            return false;
        }

        return true;
    }

    private PageInfo? GetPageInfo(string pageKey)
    {
        var infoKey = pageKey.Split(":")[0];
        return PageInformations.FirstOrDefault(x => x.Key == infoKey);
    }

    private MenuFlyout CreateTabPageContextFlyout(MenuItemInfo[]? infos, string pageKey)
    {
        var flyout = new MenuFlyout();

        if (infos?.Length > 0)
        {
            foreach (var info in infos)
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = info.Text,
                    Icon = IconInfoHelper.ConvertToIconElement(info.Icon),
                    Command = info.CanExecute is null ? new RelayCommand(info.Execute) : new RelayCommand(info.Execute, info.CanExecute),
                };
                flyout.Items.Add(menuItem);
            }

            flyout.Items.Add(new MenuFlyoutSeparator());
        }

        flyout.Items.Add(new MenuFlyoutItem()
        {
            Icon = new SymbolIcon(Symbol.Clear),
            Text = PoltergeistApplication.Localize($"Poltergeist/Resources/TabViewContextFlyout_Close"),
            KeyboardAcceleratorTextOverride = "Ctrl+F4",
            Command = new RelayCommand(() => TryCloseTab(pageKey)),
        });

        return flyout;
    }

    private void OnAppWindowClosed(AppWindowClosedEvent _)
    {
        if (TabView is null)
        {
            return;
        }

        var tabs = TabView.TabItems.OfType<TabViewItem>();
        foreach (var tab in tabs)
        {
            if (tab.Content is IPageClosed pageclosed)
            {
                pageclosed.OnPageClosed();
            }
        }
    }

}
