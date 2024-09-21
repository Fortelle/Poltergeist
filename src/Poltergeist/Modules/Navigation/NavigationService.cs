using Microsoft.UI.Xaml.Controls;
using Poltergeist.UI.Pages;

namespace Poltergeist.Modules.Navigation;

public class NavigationService : ServiceBase, INavigationService
{
    public List<NavigationInfo> Informations { get; } = new();

    public TabView? TabView { get; set; }

    public NavigationView? NavigationView { get; set; }

    public NavigationService()
    {
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

        var navkey = pageKey.Split(":")[0];

        var info = Informations.FirstOrDefault(x => x.Key == navkey);
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

        if (PoltergeistApplication.SingleMacroMode is not null && pageKey != "macro:" + PoltergeistApplication.SingleMacroMode)
        {
            Logger.Error($"Could not switch to tab page '{pageKey}' in single macro mode."); 
            return false;
        }

        var tab = TabView!.TabItems.OfType<TabViewItem>().FirstOrDefault(x => x.Tag is string s && s == pageKey);
        if (tab is null)
        {
            try
            {
                var content = info.CreateContent!.Invoke(pageKey, data);
                var header = info.CreateHeader?.Invoke(content) ?? info.Header ?? info.Text ?? info.Key;
                var icon = info.CreateIcon?.Invoke(content) ?? info.Icon?.ToIconSource();

                tab = new TabViewItem
                {
                    Header = header,
                    Content = content,
                    Tag = pageKey,
                    IconSource = icon,
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create tab page: {ex.Message}");
                PoltergeistApplication.ShowException(ex);
                return false;
            }

            if (pageKey == "home")
            {
                TabView.TabItems.Insert(0, tab);
            }
            else
            {
                TabView.TabItems.Add(tab);
            }

            Logger.Trace($"Created tab page '{pageKey}'.");
        }

        if (TabView.SelectedItem is not TabViewItem tvi || tvi != tab)
        {
            TabView.SelectedItem = tab;
        }

        Logger.Trace($"Navigated to tab page '{pageKey}'.");

        return true;
    }

    public bool TryCloseTab(string pageKey)
    {
        if (TabView is null)
        {
            return true;
        }

        var tab = TabView.TabItems.OfType<TabViewItem>().FirstOrDefault(x => x.Tag is string s && s == pageKey);

        if (tab is null)
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

        return true;
    }
}
