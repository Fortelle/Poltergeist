using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.Modules.Navigation;

public interface INavigationService
{
    TabView? TabView { get; set; }
    NavigationView? NavigationView { get; set; }

    bool NavigateTo(string pageKey, object? parameter = null);
    bool TryCloseTab(string pageKey);
    bool TryCloseTab(TabViewItem pageKey);

    void AddInfo(NavigationInfo info);
    List<NavigationInfo> Informations { get; }
}
