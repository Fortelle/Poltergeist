using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Macros;
using Poltergeist.UI;
using Poltergeist.UI.Pages;

namespace Poltergeist.Modules.Navigation;

public class NavigationService : ServiceBase, INavigationService
{
    public List<NavigationInfo> Informations { get; } = new();

    public TabView? TabView { get; set; }

    public NavigationView? NavigationView { get; set; }

    public NavigationService(AppEventService eventService)
    {
        eventService.Subscribe<AppWindowClosedEvent>(OnAppWindowClosed);
    }

    public void AddInfo(NavigationInfo info)
    {
        Informations.Add(info);

        Logger.Trace($"Added navigation information '{info.Key}'.");
    }

    public bool NavigateTo(string pageKey, object? data = null)
    {
        if (TabView is null)
        {
            return false;
        }

        var info = GetInformation(pageKey);
        if (info is null)
        {
            Logger.Error($"Failed to navigate: Invalid page key '{pageKey}'.");
            return false;
        }

        if (info.Action is not null)
        {
            Logger.Trace($"Executing the action of navagation '{pageKey}'.");
            info.Action();
            return true;
        }

        if (!CanCreateTab(pageKey))
        {
            Logger.Error($"Could not switch to tab page '{pageKey}' in exclusive macro mode.");
            return false;
        }

        if (!TryGetTab(pageKey, out var tab))
        {
            tab = CreateTabInternal(pageKey, info, data);
        }
        else if (info.UpdateArguments is not null)
        {
            var page = (Page)tab.Content;
            info.UpdateArguments.Invoke(page, data);
        }

        if (TabView.SelectedItem is not TabViewItem tvi || tvi != tab)
        {
            TabView.SelectedItem = tab;
        }

        Logger.Trace($"Navigated to tab page '{pageKey}'.");

        return true;
    }

    public bool CreateTab(string pageKey, object? data = null)
    {
        if (TabView is null)
        {
            return false;
        }

        if (!CanCreateTab(pageKey))
        {
            return false;
        }

        var info = GetInformation(pageKey);
        if (info is null)
        {
            return false;
        }

        if (TryGetTab(pageKey, out _))
        {
            return false;
        }

        var tab = CreateTabInternal(pageKey, info, data);
        return tab is not null;
    }

    private TabViewItem? CreateTabInternal(string pageKey, NavigationInfo info, object? data = null)
    {
        try
        {
            var content = info.CreateContent!.Invoke(pageKey, data);
            var header = info.CreateHeader?.Invoke(content) ?? info.Header ?? info.Text ?? info.Key;
            var icon = info.CreateIcon?.Invoke(content) ?? IconInfoHelper.ConvertToIconSource(info.Icon);
            var menuItems = info.CreateMenu?.Invoke(content) ?? info.Menu;

            var tab = new TabViewItem
            {
                Header = header,
                Content = content,
                Tag = pageKey,
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

        tab = TabView.TabItems.OfType<TabViewItem>().FirstOrDefault(x => x.Tag is string s && s == pageKey);
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

        Logger.Trace($"Removed tab page '{tab.Tag}'.");

        var pageKey = (string)tab.Tag;
        PoltergeistApplication.GetService<AppEventService>().Publish(new AppWindowPageClosedEvent(pageKey));

        return true;
    }

    private bool CanCreateTab(string pageKey)
    {
        if (!string.IsNullOrEmpty(PoltergeistApplication.Current.ExclusiveMacroMode) && pageKey != MacroManager.GetPageKey(PoltergeistApplication.Current.ExclusiveMacroMode))
        {
            return false;
        }

        return true;
    }

    private NavigationInfo? GetInformation(string pageKey)
    {
        var navkey = pageKey.Split(":")[0];
        return Informations.FirstOrDefault(x => x.Key == navkey);
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
