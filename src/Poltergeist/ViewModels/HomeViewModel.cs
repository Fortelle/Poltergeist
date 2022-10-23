using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Automations.Macros;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public class HomeViewModel : ObservableRecipient
{
    private IMacroBase[] _recentMacros;
    private bool _enableRecentMacro;

    public IMacroBase[] RecentMacros { get => _recentMacros; set => SetProperty(ref _recentMacros, value); }
    public bool EnableRecentMacro { get => _enableRecentMacro; set => SetProperty(ref _enableRecentMacro, value); }

    public List<MacroGroup> Groups { get; set; }

    public ICommand ClearRecentsCommand { get; }

    public HomeViewModel(MacroManager macroManager, LocalSettingsService localSettings)
    {
        Groups = macroManager.Groups;

        ClearRecentsCommand = new RelayCommand(ClearRecentMacros);
        EnableRecentMacro = App.GetSettings<int>("app.maxrecentmacros", 0) > 0;
        UpdateRecentMacros();

        localSettings.Changed += (key, value) => {
            if (key == "app.maxrecentmacros")
            {
                EnableRecentMacro = (int)value > 0;
            }
        };
    }

    public void UpdateRecentMacros()
    {
        var macroManager = App.GetService<MacroManager>();
        RecentMacros = macroManager.RecentMacros.Select(x => macroManager.GetMacro(x)).Reverse().ToArray();
    }

    public void ClearRecentMacros()
    {
        var macroManager = App.GetService<MacroManager>();
        macroManager.RecentMacros.Clear();
        App.SetSettings("app.recentmacros", Array.Empty<string>(), save: true);
        UpdateRecentMacros();
    }

}
