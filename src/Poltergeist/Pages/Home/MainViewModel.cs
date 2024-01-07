using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Automations.Macros;
using Poltergeist.Contracts.Services;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private const int MaxItemCount = 10;

    public List<MacroGroup> Groups { get; }

    [ObservableProperty]
    private MacroSummaryEntry[]? _recentMacros;

    [ObservableProperty]
    private MacroSummaryEntry[]? _favoriteMacros;

    [ObservableProperty]
    private MacroSummaryEntry[]? _popularMacros;

    public MainViewModel(MacroManager macroManager)
    {
        Groups = macroManager.Groups;
    }

    [RelayCommand]
    private static void MacroButtonClick(string macrokey)
    {
        var nav = App.GetService<INavigationService>();
        nav.NavigateTo("macro:" + macrokey);
    }

    [RelayCommand]
    private static void GroupButtonClick(string groupkey)
    {
        var nav = App.GetService<INavigationService>();
        nav.NavigateTo("group:" + groupkey);
    }

    public void UpdateMacros()
    {
        var macroManager = App.GetService<MacroManager>();

        FavoriteMacros = macroManager.Summaries.Values
            .Where(x => x.IsFavorite)
            .Where(x => macroManager.GetMacro(x.MacroKey) != null)
            .ToArray();

        RecentMacros = macroManager.Summaries.Values
            .Where(x => x.LastRunTime != default)
            .OrderByDescending(x => x.LastRunTime)
            .Where(x => macroManager.GetMacro(x.MacroKey) != null)
            .Take(MaxItemCount)
            .ToArray();

        PopularMacros = macroManager.Summaries.Values
            .Where(x => x.RunCount != default)
            .OrderByDescending(x => x.RunCount)
            .Where(x => macroManager.GetMacro(x.MacroKey) != null)
            .Take(MaxItemCount)
            .ToArray();
    }

}
