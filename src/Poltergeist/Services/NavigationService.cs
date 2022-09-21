using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Poltergeist.Automations.Macros;
using Poltergeist.ViewModels;
using Poltergeist.Views;

namespace Poltergeist.Services;

public class NavigationService
{
    public event Action<Page> Navigated;

    private readonly Dictionary<string, Page> Pages = new();

    public NavigationService()
    {
    }

    public void Navigate(string pageKey, object parameter = null, bool clearNavigation = false)
    {
        if (string.IsNullOrEmpty(pageKey)) return;

        var page = GetPage(pageKey);
        Navigated?.Invoke(page);
    }

    private Page GetPage(string pageKey)
    {
        pageKey = pageKey.ToLower();

        if (Pages.ContainsKey(pageKey))
        {
            return Pages[pageKey];
        }

        Page page = pageKey switch
        {
            "home" => App.GetService<HomePage>(),
            "settings" => App.GetService<SettingsPage>(),
            "console" => App.GetService<MacroConsolePage>(),
            "develop" => App.GetService<DevelopPage>(),
            "scheduler" => App.GetService<SchedulerPage>(),
            "about" => App.GetService<AboutPage>(),
            var x when x.StartsWith("group_") => CreateGroupPage(x),
            _ => throw new ArgumentException($"Page not found: {pageKey}."),
        };
        page.Name = pageKey;
        Pages.Add(pageKey, page);
        return page;
    }

    private MacroGroupPage CreateGroupPage(string key)
    {
        var groupName = key.Replace("group_", "");

        var manager = App.GetService<MacroManager>();
        var page = App.GetService<MacroGroupPage>();
        var group = manager.Groups.First(x => x.Name.ToLower() == groupName);
        page.DataContext = group;

        return page;
    }
}
