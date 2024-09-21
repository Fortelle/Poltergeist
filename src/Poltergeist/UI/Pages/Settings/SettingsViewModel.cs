using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Settings;

namespace Poltergeist.UI.Pages.Settings;

public partial class SettingsViewModel : ObservableRecipient
{
    public ObservableParameterCollection AppSettings { get; set; }

    public ObservableParameterCollection GlobalOptions { get; set; }

    public SettingsViewModel(AppSettingsService appSettings, MacroManager macroManager)
    {
        AppSettings = new(appSettings.Settings);
        GlobalOptions = new(macroManager.GlobalOptions);
    }

}
