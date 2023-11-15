using Microsoft.UI.Xaml.Controls;
using Poltergeist.Contracts.Services;

namespace Poltergeist.Services;

public class NavigationService : INavigationService
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
    }

    public NavigationInfo? GetInfo(string key)
    {
        return Informations.FirstOrDefault(x => x.Key == key);
    }

    public bool NavigateTo(string pageKey, object? parameter = null)
    {
        if (TabView == null)
        {
            return false;
        }
        if(App.SingleMacroMode is not null && pageKey != "macro:" + App.SingleMacroMode)
        {
            return false;
        }

        var keyparts = pageKey.Split(":");

        var tab = TabView.TabItems.OfType<TabViewItem>().FirstOrDefault(x => x.Tag is string s && s == pageKey);
        
        if (tab is null)
        {
            var info = GetInfo(keyparts[0]);
            if(info is null)
            {
                return false;
            }

            var content = info.CreateContent?.Invoke(keyparts[1..], parameter);
            if (content is null)
            {
                return false;
            }

            var header = info.CreateHeader?.Invoke(content);
            header ??= info.Header;
            header ??= App.Localize($"Poltergeist/Resources/TabHeader_{info.Key}");
            if (header is null || (header is string s && string.IsNullOrEmpty(s)))
            {
                header = " ";
            }

            tab = new TabViewItem
            {
                Header = header,
                Content = content,
                
                IconSource = new FontIconSource()
                {
                    Glyph = info.Glyph,
                },
                Tag = pageKey
            };

            if(pageKey == "home")
            {
                TabView.TabItems.Insert(0, tab);
            }
            else
            {
                TabView.TabItems.Add(tab);
            }
        }

        if (TabView.SelectedItem is not TabViewItem tvi || tvi != tab)
        {
            TabView.SelectedItem = tab;
        }

        return true;
    }

}
