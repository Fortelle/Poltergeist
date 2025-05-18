using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;
using Poltergeist.Modules.Settings;

namespace Poltergeist.Helpers;

/// <summary>
/// When the application starts, restores the previously opened tabs from the last time.
/// </summary>
/// <remarks>
/// If command line argument "--macro" is used, this feature will be ignored.
/// </remarks>
public static class RestorePreviousTabsHelper
{
    private const string ConfigKey = "restore_previous_tabs";
    private const string DataKey = "previous_tabs";

    public static bool IsEnabled { get; set; } = true;

    public static void Inject()
    {
        var eventService = PoltergeistApplication.GetService<AppEventService>();
        eventService.Subscribe<AppWindowLoadedHandler>(OnAppWindowLoaded);
        eventService.Subscribe<AppSettingsSavingHandler>(OnAppSettingsSaving);
    }

    private static void OnAppWindowLoaded(AppWindowLoadedHandler e)
    {
        var settingsService = PoltergeistApplication.GetService<AppSettingsService>();
        settingsService.Add(new OptionDefinition<bool>(ConfigKey, false)
        {
            Category = PoltergeistApplication.Localize($"Poltergeist/Resources/AppSettings_App"),
            DisplayLabel = PoltergeistApplication.Localize($"Poltergeist/Resources/AppSettings_App_RestorePreviousTabs"),
        });

        if (IsEnabled && settingsService.Get<bool>(ConfigKey))
        {
            var openedPages = settingsService.Get<string[]>(DataKey);
            if (openedPages?.Length > 0)
            {
                var navigationService = PoltergeistApplication.GetService<INavigationService>();
                var macroManager = PoltergeistApplication.GetService<MacroManager>();
                foreach (var pageKey in openedPages)
                {
                    if (pageKey.StartsWith("macro:"))
                    {
                        var shellKey = pageKey.Split(':', 2)[1];
                        if (macroManager.GetShell(shellKey)?.Template is null)
                        {
                            continue;
                        }
                        navigationService.NavigateTo(pageKey);
                    }
                }
            }
        }

        settingsService.Remove(DataKey);
    }

    private static void OnAppSettingsSaving(AppSettingsSavingHandler e)
    {
        if (IsEnabled && e.Settings.Get<bool>(ConfigKey))
        {
            var navigationService = PoltergeistApplication.GetService<INavigationService>();
            var openedTabs = navigationService.TabView?.TabItems
                .OfType<TabViewItem>()
                .Where(x => x.Tag is string s && s.StartsWith("macro:"))
                .Select(x => (string)x.Tag)
                .ToArray();
            if (openedTabs?.Length > 0)
            {
                e.Settings.Set(DataKey, openedTabs);
            }
        }
        else
        {
            e.Settings.Remove(DataKey);
        }
    }

}
