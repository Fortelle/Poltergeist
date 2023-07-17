using Microsoft.UI.Xaml.Controls;
using Poltergeist.Services;

namespace Poltergeist.Contracts.Services;

public interface INavigationService
{
    TabView? TabView
    {
        get; set;
    }

    NavigationView? NavigationView
    {
        get; set;
    }

    bool NavigateTo(string pageKey, object? parameter = null);

    void AddInfo(NavigationInfo info);
    NavigationInfo? GetInfo(string key);
}
