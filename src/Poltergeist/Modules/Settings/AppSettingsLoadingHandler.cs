using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Settings;

public class AppSettingsLoadingHandler(AppSettingsService service) : AppEventHandler
{
    public AppSettingsService Service => service;
}
