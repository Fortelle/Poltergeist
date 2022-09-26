using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Configs;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    public MacroOptions LocalSettings { get; set; }
    public MacroOptions MacroOptions { get; set; }

    public SettingsViewModel(LocalSettingsService localSettings, MacroManager macroManager)
    {
        LocalSettings = localSettings.Settings;
        MacroOptions = macroManager.GlobalOptions;
    }

}
