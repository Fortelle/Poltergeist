using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Poltergeist.Automations.Instruments;

public class GridInstrumentViewModel : InstrumentItemViewModel
{
    public string Title { get; set; }
    public int Columns { get; set; }
    public int Size { get; set; }

    protected readonly Dictionary<string, SolidColorBrush> ForeColors = new();
    protected readonly Dictionary<string, SolidColorBrush> BackColors = new();

    public ObservableCollection<GridInstrumentItemViewModel> Items { get; set; } = new();

    public GridInstrumentViewModel(GridInstrument gi)
    {
        Columns = gi.Columns;
        Size = gi.Size;
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

        foreach (var item in gi.Presets)
        {
            var vm = ToItemViewModel(item);
            Items.Add(vm);
        }
    }

    private void Gi_Changed(int index, GridInstrumentItem item)
    {
        var vm = ToItemViewModel(item);

        if(index == -1)
        {
            Items.Add(vm);
        }
        else
        {
            Items[index] = vm;
        }
    }

    private GridInstrumentItemViewModel ToItemViewModel(GridInstrumentItem item)
    {
        var vm = new GridInstrumentItemViewModel(item)
        {
            Size = Size,
        };

        if (ForeColors.TryGetValue(vm.Status.ToString(), out var forecolor)) vm.Foreground = forecolor;
        if (BackColors.TryGetValue(vm.Status.ToString(), out var backcolor)) vm.Background = backcolor;

        return vm;
    }

}
