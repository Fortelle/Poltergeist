using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Automations.Macros;
using Poltergeist.Contracts.Services;
using Poltergeist.Services;

namespace Poltergeist.Pages.Groups;

// todo: show fav icon
public partial class MacroGroupViewModel : ObservableRecipient
{
    public MacroGroup Group { get; set; }
    public IMacroBase[] Macros { get; set; }

    public MacroGroupViewModel(MacroGroup group)
    {
        Group = group;

        var macroManager = App.GetService<MacroManager>();
        Macros = group.Macros
            .OrderByDescending(x => macroManager.GetSummary(x.Key)?.IsFavorite == true)
            .ThenBy(x => x.Title)
            .ToArray();
    }

    [RelayCommand]
    private static void MacroButtonClick(string macrokey)
    {
        var nav = App.GetService<INavigationService>();
        nav.NavigateTo("macro:" + macrokey);
    }

}
