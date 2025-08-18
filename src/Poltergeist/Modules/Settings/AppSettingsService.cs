using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Settings;

public class AppSettingsService : ServiceBase
{
    public SavablePredefinedCollection Settings => PoltergeistApplication.Current.Settings;
    public SavablePredefinedCollection InternalSettings => PoltergeistApplication.Current.InternalSettings;

    public AppSettingsService(AppEventService eventService)
    {
        eventService.Subscribe<AppWindowClosedEvent>(OnAppWindowClosed);
    }

    public void WatchChange<T>(string key, Action<T> action)
    {
        PoltergeistApplication.GetService<AppEventService>().Subscribe<AppSettingsChangedEvent>(e =>
        {
            if (e.Key == key)
            {
                var newValue = (T)e.NewValue!;
                action(newValue);
            }
        });
    }

    public void WatchChange<T>(string key, Action<T?, T?> action)
    {
        PoltergeistApplication.GetService<AppEventService>().Subscribe<AppSettingsChangedEvent>(e =>
        {
            if (e.Key == key)
            {
                var oldValue = (T?)e.OldValue;
                var newValue = (T?)e.NewValue;
                action(oldValue, newValue);
            }
        });
    }

    private void OnAppWindowClosed(AppWindowClosedEvent _)
    {
        try
        {
            Settings.Save();
        }
        catch
        {
        }

        try
        {
            InternalSettings.Save();
        }
        catch
        {
        }
    }
}
