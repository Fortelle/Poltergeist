using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;
using Poltergeist.Modules.Settings;
using Poltergeist.UI.Pages.Macros;

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
    private const string DataKey = "previous_instances";

    public static bool IsEnabled { get; set; } = true;

    public static void Inject()
    {
        var eventService = PoltergeistApplication.GetService<AppEventService>();
        eventService.Subscribe<AppWindowLoadedEvent>(OnAppWindowLoaded);
        eventService.Subscribe<AppWindowClosedEvent>(OnAppWindowClosed, new() { Priority = 100 });
    }

    private static void OnAppWindowLoaded(AppWindowLoadedEvent _)
    {
        var settingsService = PoltergeistApplication.GetService<AppSettingsService>();
        settingsService.Settings.AddDefinition(new OptionDefinition<bool>(ConfigKey, false)
        {
            Category = PoltergeistApplication.Localize($"Poltergeist/Resources/AppSettings_App"),
            DisplayLabel = PoltergeistApplication.Localize($"Poltergeist/Resources/AppSettings_App_RestorePreviousTabs"),
        });

        if (IsEnabled && settingsService.Settings.Get<bool>(ConfigKey))
        {
            var previousInstances = settingsService.InternalSettings.Get<string[]>(DataKey);
            if (previousInstances?.Length > 0)
            {
                var instanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();
                var macroManager = PoltergeistApplication.GetService<MacroManager>();
                foreach (var instanceId in previousInstances)
                {
                    var instance = instanceManager.GetInstance(instanceId);
                    if (instance is not null)
                    {
                        macroManager.OpenPage(instance);
                    }
                }
            }
        }
    }

    private static void OnAppWindowClosed(AppWindowClosedEvent _)
    {
        var settingsService = PoltergeistApplication.GetService<AppSettingsService>();
        if (IsEnabled && settingsService.Settings.Get<bool>(ConfigKey))
        {
            var navigationService = PoltergeistApplication.GetService<INavigationService>();
            var openedTabs = navigationService.TabView?.TabItems
                .OfType<TabViewItem>()
                .Where(x => x.Content is MacroPage macroPage && macroPage.ViewModel.Instance.IsPersistent)
                .Select(x => ((MacroPage)x.Content).ViewModel.Instance.InstanceId)
                .ToArray();
            if (openedTabs?.Length > 0)
            {
                settingsService.InternalSettings.Set(DataKey, openedTabs);
            }
        }
        else
        {
            settingsService.InternalSettings.Remove(DataKey);
        }
    }
}
