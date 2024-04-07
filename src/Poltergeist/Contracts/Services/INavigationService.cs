using Microsoft.UI.Xaml.Controls;
using Poltergeist.Services;

namespace Poltergeist.Contracts.Services;

public interface INavigationService
{
    TabView? TabView { get; set; }
    NavigationView? NavigationView { get; set; }

    bool NavigateTo(string pageKey, object? parameter = null);
    bool TryCloseTab(string pageKey);
    bool TryCloseTab(TabViewItem pageKey);

    void AddInfo(NavigationInfo info);
    NavigationInfo? GetInfo(string key);
}
