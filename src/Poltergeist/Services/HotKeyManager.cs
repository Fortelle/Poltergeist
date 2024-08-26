using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Services;

public class HotKeyManager
{
    private const string StartKey = "macro.startkey";

    public HotKeyManager()
    {
        App.SettingsLoading += (settings) =>
        {
            settings.Add(new OptionDefinition<HotKey>(StartKey)
            {
                Category = ResourceHelper.Localize("Poltergeist/Resources/LocalSettings_Macro"),
                DisplayLabel = ResourceHelper.Localize("Poltergeist/Resources/LocalSettings_Macro_StartMacroHotKey"),
            });
        };

        App.ContentLoaded += LoadHotKey;

    }

    private void LoadHotKey()
    {
        var localSettings = App.GetService<LocalSettingsService>();
        var startHotkey = localSettings.Get<HotKey>(StartKey);

        if (startHotkey.KeyCode == VirtualKey.None)
        {
            return;
        }

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var hotkeyService = App.GetService<HotKeyService>();
            hotkeyService.Register(startHotkey, () =>
            {
                ActionService.RunMacro(Automations.Processors.LaunchReason.ByUser, true);
            });
        });
    }
}
