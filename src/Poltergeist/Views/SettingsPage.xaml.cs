using System.Windows.Controls;
using Poltergeist.Services;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; set; }

    public SettingsPage(SettingsViewModel vm)
    {
        ViewModel = vm;
        DataContext = vm;

        InitializeComponent();
    }

    private void MacroConfigControl_ItemUpdated(object sender, ConfigUpdatedArguments e)
    {
        var manager = App.GetService<MacroManager>();
        manager.SaveGlobalOptions();

        App.ShowFlyout("Global options saved");
    }

    private void MacroConfigControl_ItemUpdated2(object sender, ConfigUpdatedArguments e)
    {
        var localSettings = App.GetService<LocalSettingsService>();
        localSettings.Save();

        localSettings.OnChanged(e.Key, e.Value);

        App.ShowFlyout("Local settings saved");
    }
}
