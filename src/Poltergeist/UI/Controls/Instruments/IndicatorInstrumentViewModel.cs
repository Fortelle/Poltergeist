using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Helpers;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class IndicatorInstrumentViewModel : IInstrumentViewModel
{
    public const int DefaultIconSize = 48;

    public string? Title { get; set; }
    public int IconWidth { get; set; }
    public int IconHeight { get; set; }
    public int FontSize { get; set; }

    public SynchronizableCollection<IndicatorInstrumentItem, IndicatorInstrumentItemViewModel> Items { get; set; }

    public IndicatorInstrumentViewModel(IndicatorInstrument model)
    {
        Title = model.Title;
        IconWidth = model.IconSize ?? DefaultIconSize;
        IconHeight = model.IconSize ?? DefaultIconSize;
        FontSize = IconWidth / 2;

        Items = new(model.Items, ModelToViewModel, PoltergeistApplication.Current.DispatcherQueue);
    }

    private IndicatorInstrumentItemViewModel? ModelToViewModel(IndicatorInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new IndicatorInstrumentItemViewModel(item);

        return vm;
    }
}
