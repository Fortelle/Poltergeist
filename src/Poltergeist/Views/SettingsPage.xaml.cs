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

    private void MacroConfigControl_ItemUpdated(object sender, System.EventArgs e)
    {
        var manager = App.GetService<MacroManager>();
        manager.SaveGlobalOptions();

        App.ShowFlyout("Global options saved");
    }

    private void MacroConfigControl_ItemUpdated2(object sender, System.EventArgs e)
    {
        var manager = App.GetService<LocalSettingsService>();
        manager.Save();

        App.ShowFlyout("Local settings saved");
    }
}
