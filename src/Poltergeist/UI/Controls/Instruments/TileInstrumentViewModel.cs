using Poltergeist.Automations.Components.Panels;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class TileInstrumentViewModel : IInstrumentViewModel
{
    public const int DefaultIconSize = 48;

    public string? Title { get; set; }

    public int IconWidth { get; set; }

    public int IconHeight { get; set; }

    public double FontSize { get; set; }

    public SynchronizableCollection<TileInstrumentItem, TileInstrumentItemViewModel> Items { get; set; }

    public TileInstrumentViewModel(ITileInstrumentModel model)
    {
        Title = model.Title;
        IconWidth = model.IconWidth ?? model.IconSize ?? DefaultIconSize;
        IconHeight = model.IconHeight ?? model.IconSize ?? DefaultIconSize;
        FontSize = Math.Min(IconWidth, IconHeight) / 2;

        Items = new(model.Items, ModelToViewModel, PoltergeistApplication.Current.DispatcherQueue);
    }

    private TileInstrumentItemViewModel? ModelToViewModel(TileInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new TileInstrumentItemViewModel(item);

        return vm;
    }
}
