using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Settings;

namespace Poltergeist.UI.Pages.Settings;

public partial class SettingsViewModel : ObservableRecipient, IDisposable
{
    private const int SettingsSaveDelaySeconds = 30;

    public ObservableParameterCollection AppSettings { get; set; }

    public ObservableParameterCollection GlobalOptions { get; set; }

    private readonly Debouncer SaveDebouncer;

    public SettingsViewModel(
        AppSettingsService appSettings,
        GlobalOptionsService globalOptionsService
        )
    {
        AppSettings = new(appSettings.Settings);
        AppSettings.Changed += (key, oldValue, newValue) =>
        {
            PoltergeistApplication.GetService<AppEventService>().Publish(new AppSettingsChangedEvent()
            {
                Key = key,
                OldValue = oldValue,
                NewValue = newValue,
            });
            Save();
        };

        GlobalOptions = new(globalOptionsService.GlobalOptions);
        GlobalOptions.Changed += (key, oldValue, newValue) =>
        {
            Save();
        };

        SaveDebouncer = new(() => {
            PoltergeistApplication.GetService<AppSettingsService>().Settings.Save();
            PoltergeistApplication.GetService<GlobalOptionsService>().Save();
        }, TimeSpan.FromSeconds(SettingsSaveDelaySeconds));
    }

    public void Save(bool forceExecute = false)
    {
        SaveDebouncer.Trigger(forceExecute);
    }

    public void Dispose()
    {
        SaveDebouncer.Dispose();
    }
}
