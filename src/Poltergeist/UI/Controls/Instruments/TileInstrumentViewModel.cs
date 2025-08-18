using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Helpers;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class TileInstrumentViewModel : IInstrumentViewModel
{
    public const int DefaultIconSize = 48;

    public string? Title { get; set; }
    public int IconWidth { get; set; }
    public int IconHeight { get; set; }

    public SynchronizableCollection<TileInstrumentItem, TileInstrumentItemViewModel> Items { get; set; }

    public TileInstrumentViewModel(ITileInstrumentModel model)
    {
        Title = model.Title;
        IconWidth = model.IconWidth ?? model.IconSize ?? DefaultIconSize;
        IconHeight = model.IconHeight ?? model.IconSize ?? DefaultIconSize;

        Items = new(model.Items, ModelToViewModel, PoltergeistApplication.Current.DispatcherQueue);
    }

    private TileInstrumentItemViewModel? ModelToViewModel(TileInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new TileInstrumentItemViewModel(item);

        if (item.Color is not null && ThemeColors.Colors.TryGetValue(item.Color.Value, out var colorset))
        {
            vm.Foreground = new SolidColorBrush(ColorUtil.ToColor(colorset.Foreground));
            vm.Background = new SolidColorBrush(ColorUtil.ToColor(colorset.Background));
        }

        return vm;
    }
}
