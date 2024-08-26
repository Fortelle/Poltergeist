using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    public ObservableParameterCollection LocalSettings { get; set; }

    public ObservableParameterCollection GlobalOptions { get; set; }

    public SettingsViewModel(LocalSettingsService localSettings, MacroManager macroManager)
    {
        LocalSettings = new(localSettings.Settings);
        GlobalOptions = new(macroManager.GlobalOptions);
    }

}
