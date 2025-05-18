using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Settings;

public class AppSettingsService : ServiceBase
{
    private static string? FilePath { get; set; }
    private static Exception? LoadingException { get; set; }

    private static readonly ParameterDefinitionValueCollection settings = new();
    public ParameterDefinitionValueCollection Settings => settings;

    public static void Load()
    {
        FilePath = Path.Combine(PoltergeistApplication.Paths.DocumentDataFolder, "Settings.json");

        try
        {
            settings.Load(FilePath);

            PoltergeistApplication.GetService<AppEventService>().Raise(new AppSettingsLoadedHandler(settings));
        }
        catch (Exception ex)
        {
            LoadingException = ex;
        }
    }

    public AppSettingsService(AppEventService eventService)
    {
        eventService.Subscribe<AppContentLoadingHandler>(OnAppContentLoading);
        eventService.Subscribe<AppWindowClosedHandler>(OnAppWindowClosed);
    }

    public void Add(IParameterDefinition definition)
    {
        Settings.Add(definition);
        Logger.Trace($"Added application settings definition '{definition.Key}'.", new { definition });
    }

    public T? Get<T>(string key)
    {
        return Settings.Get<T>(key);
    }

    public T? Get<T>(ParameterDefinition<T> definition)
    {
        return Settings.Get<T>(definition.Key);
    }

    public void Set<T>(string key, T value)
    {
        Settings.Set(key, value);
        Logger.Trace($"Set the application settings variable: {key} = {value}");

        PoltergeistApplication.GetService<AppEventService>().Raise(new AppSettingsChangedHandler() {
            Key = key,
            NewValue = value,
        });
    }

    private void OnAppContentLoading(AppContentLoadingHandler e)
    {
        switch (LoadingException)
        {
            case FileNotFoundException:
                Logger.Trace($"The application settings were not loaded: File does not exist.", new { FilePath });
                break;
            case Exception ex:
                Logger.Warn($"The application settings were not loaded successfully: {ex.Message}");
                break;
            default:
                Logger.Trace($"The application settings were loaded successfully.", new { FilePath });
                break;
        }
    }

    private void OnAppWindowClosed(AppWindowClosedHandler e)
    {
        PoltergeistApplication.GetService<AppEventService>().Raise(new AppSettingsSavingHandler(Settings));

        try
        {
            Settings.Save();

            Logger.Trace("Saved the application settings.", new { FilePath });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save the application settings: {ex.Message}");
        }
    }
}
