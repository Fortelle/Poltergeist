using Poltergeist.Automations.Configs;
using Poltergeist.Input.Windows;

namespace Poltergeist.Services;

public class HotKeyManager
{
    private const string StartKey = "macro.startkey";

    public HotKeyManager()
    {
        App.SettingsLoading += (settings) =>
        {
            settings.Add(new OptionItem<HotKey>(StartKey)
            {
                Category = "Macro",
                DisplayLabel = "Start Macro HotKey",
            });
        };

        App.ContentLoaded += LoadHotKey;

    }

    private void LoadHotKey()
    {
        var localSettings = App.GetService<LocalSettingsService>();
        var startHotkey = localSettings.Get<HotKey>("macro.startkey");

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
