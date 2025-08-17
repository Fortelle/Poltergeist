using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.Modules.Navigation;

public interface INavigationService
{
    TabView? TabView { get; set; }
    NavigationView? NavigationView { get; set; }

    bool NavigateTo(string pageKey, object? parameter = null);
    bool TryCloseTab(string pageKey);
    bool TryCloseTab(TabViewItem pageKey);
    bool TryGetTab(string pageKey, [MaybeNullWhen(false)] out TabViewItem? tab);

    void AddInfo(NavigationInfo info);
    List<NavigationInfo> Informations { get; }
}
