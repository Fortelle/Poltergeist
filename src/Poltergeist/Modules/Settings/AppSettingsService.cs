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
