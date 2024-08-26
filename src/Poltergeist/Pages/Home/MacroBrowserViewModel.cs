using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Pages.Home;
using Poltergeist.Services;

namespace Poltergeist.Pages.Groups;

public partial class MacroBrowserViewModel : ObservableRecipient
{
    public const string LastSortIndexKey = "last_sort_index";

    [ObservableProperty]
    private MacroShell[]? _macros;

    private int SortIndex;

    public MacroBrowserViewModel()
    {
        var localSettings = App.GetService<LocalSettingsService>();
        SortIndex = localSettings.Get<int>(LastSortIndexKey);

        RefreshMacroList();

        var macroManager = App.GetService<MacroManager>();
        macroManager.MacroCollectionChanged += RefreshMacroList;
        macroManager.MacroPropertyChanged += RefreshMacroList;
    }

    private void RefreshMacroList()
    {
        var macroManager = App.GetService<MacroManager>();
        var macros = macroManager.Shells.AsEnumerable();
        if (!App.IsDevelopment)
        {
            macros = macros.Where(x => x.Template is not null);
        }
        macros = SortIndex switch
        {
            1 => macros.OrderByDescending(x => x.Properties.IsFavorite).ThenBy(x => x.Title),
            -1 => macros.OrderByDescending(x => x.Properties.IsFavorite).ThenByDescending(x => x.Title),
            2 => macros.OrderBy(x => x.Properties.RunCount is null).ThenBy(x => x.Properties.RunCount),
            -2 => macros.OrderBy(x => x.Properties.RunCount is null).ThenByDescending(x => x.Properties.RunCount),
            3 => macros.OrderBy(x => x.Properties.LastRunTime is null).ThenBy(x => x.Properties.LastRunTime),
            -3 => macros.OrderBy(x => x.Properties.LastRunTime is null).ThenByDescending(x => x.Properties.LastRunTime),
            _ => macros,
        };
        Macros = macros.ToArray();
    }

    [RelayCommand]
    private async void NewMacro()
    {
        var editor = new MacroEditor(true);
        var contentDialog = new ContentDialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/NewMacroDialog_Title"),
            Content = editor,
            Valid = () => editor.SelectedTemplateKey is null ? "" : null
        };

        await DialogService.ShowAsync(contentDialog);

        if (contentDialog.Result == DialogResult.Cancel)
        {
            return;
        }

        var macroManager = App.GetService<MacroManager>();
        var macro = macroManager.GetTemplate(editor.SelectedTemplateKey!)!;
        var newShell = new MacroShell(macro);
        newShell.Properties.Title = editor.MacroName;
        macroManager.AddMacro(newShell);
        macroManager.SaveProperties();
    }

    [RelayCommand]
    public void Sort(int index)
    {
        if (index == 0)
        {
            return;
        }

        if (SortIndex == index)
        {
            SortIndex = -index;
        }
        else
        {
            SortIndex = index;
        }

        RefreshMacroList();

        App.GetService<LocalSettingsService>().Set(LastSortIndexKey, SortIndex);
    }
}
