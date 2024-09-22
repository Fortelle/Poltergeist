using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Interactions;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Settings;

namespace Poltergeist.UI.Pages.Home;

public partial class MacroBrowserViewModel : ObservableRecipient, IDisposable
{
    public const string LastSortIndexKey = "last_sort_index";

    [ObservableProperty]
    private MacroShell[]? _macros;

    private int SortIndex;
    private bool disposedValue;

    public MacroBrowserViewModel()
    {
        var settings = App.GetService<AppSettingsService>();
        SortIndex = settings.Get<int>(LastSortIndexKey);

        RefreshMacroList();

        App.GetService<AppEventService>().Subscribe<MacroCollectionChangedHandler>(MacroCollectionChanged);
        App.GetService<AppEventService>().Subscribe<MacroPropertyChangedHandler>(OnMacroPropertyChanged);
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
            1 => macros.OrderBy(x => x.Title),
            -1 => macros.OrderByDescending(x => x.Title),
            2 => macros.OrderBy(x => x.Properties.RunCount is null).ThenBy(x => x.Properties.RunCount),
            -2 => macros.OrderBy(x => x.Properties.RunCount is null).ThenByDescending(x => x.Properties.RunCount),
            3 => macros.OrderBy(x => x.Properties.LastRunTime is null).ThenBy(x => x.Properties.LastRunTime),
            -3 => macros.OrderBy(x => x.Properties.LastRunTime is null).ThenByDescending(x => x.Properties.LastRunTime),
            _ => macros,
        };

        macros = macros.OrderByDescending(x => x.Properties.IsFavorite);

        Macros = macros.ToArray();
    }

    [RelayCommand]
    private async void NewMacro()
    {
        var macroManager = App.GetService<MacroManager>();
        var templates = macroManager.Templates.ToDictionary(x => x.Key, x => x.Title);
        var editor = new MacroEditor(templates, true);

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

        if (string.IsNullOrEmpty(editor.SelectedTemplateKey))
        {
            return;
        }

        var macro = macroManager.GetTemplate(editor.SelectedTemplateKey)!;
        var newShell = new MacroShell(macro);
        newShell.Properties.Title = editor.MacroName;
        macroManager.NewMacro(newShell);
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

        App.GetService<AppSettingsService>().Set(LastSortIndexKey, SortIndex);
    }

    private void OnMacroPropertyChanged(MacroPropertyChangedHandler handler)
    {
        RefreshMacroList();
    }

    private void MacroCollectionChanged(MacroCollectionChangedHandler handler)
    {
        RefreshMacroList();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
        {
            return;
        }

        if (disposing)
        {
            App.GetService<AppEventService>().Unsubscribe<MacroCollectionChangedHandler>(MacroCollectionChanged);
            App.GetService<AppEventService>().Unsubscribe<MacroPropertyChangedHandler>(OnMacroPropertyChanged);
        }

        disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
