using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Poltergeist.Automations.Instruments;

public class ListInstrumentViewModel : InstrumentItemViewModel
{
    public string Title { get; set; }

    protected readonly Dictionary<string, SolidColorBrush> ForeColors = new();
    protected readonly Dictionary<string, SolidColorBrush> BackColors = new();
    protected readonly Dictionary<string, string> Glyphs = new();

    public ObservableCollection<ListInstrumentItemViewModel> Items { get; set; } = new();

    public ListInstrumentViewModel(ListInstrument gi)
    {
        Title = gi.Title;
        gi.Changed += Gi_Changed;

        foreach (var (key, color) in gi.Colors)
        {
            var brush = new SolidColorBrush(color.Foreground);
            brush.Freeze();
            ForeColors.Add(key, brush);
        }
        foreach (var (key, color) in gi.Colors)
        {
            var brush = new SolidColorBrush(color.Background);
            brush.Freeze();
            BackColors.Add(key, brush);
        }
        foreach (var (key, color) in gi.Colors)
        {
            Glyphs.Add(key, color.Icon);
        }

        foreach (var item in gi.Presets)
        {
            var vm = ToItemViewModel(item);
            Items.Add(vm);
        }
    }

    private void Gi_Changed(int index, ListInstrumentItem item)
    {
        var vm = ToItemViewModel(item);

        if (index == -1)
        {
            Items.Add(vm);
        }
        else if (index == Items.Count)
        {
            Items.Add(vm);
        }
        else if (index < Items.Count)
        {
            Items[index] = vm;
        }
    }

    private ListInstrumentItemViewModel ToItemViewModel(ListInstrumentItem item)
    {
        var vm = new ListInstrumentItemViewModel(item)
        {
        };
        var status = vm.Status.ToString();

        if (ForeColors.TryGetValue(status, out var forecolor)) vm.Foreground = forecolor;
        if (BackColors.TryGetValue(status, out var backcolor)) vm.Background = backcolor;
        if (Glyphs.TryGetValue(status, out var glyph)) vm.Glyph = glyph;

        return vm;
    }

}
