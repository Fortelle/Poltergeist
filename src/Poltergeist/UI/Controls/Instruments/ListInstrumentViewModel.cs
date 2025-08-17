using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class ListInstrumentViewModel : IInstrumentViewModel
{
    public string? Title { get; set; }
    public SynchronizableCollection<ListInstrumentItem, ListInstrumentItemViewModel> Items { get; set; }

    public ListInstrumentViewModel(IListInstrumentModel model)
    {
        Title = model.Title;

        Items = new(model.Items, ModelToViewModel, PoltergeistApplication.Current.DispatcherQueue);
    }

    private ListInstrumentItemViewModel? ModelToViewModel(ListInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new ListInstrumentItemViewModel(item);

        if (item.Color is not null)
        {
            var colorset = ThemeColors.Colors[item.Color.Value];
            vm.Foreground = new SolidColorBrush(colorset.Foreground);
            vm.Background = new SolidColorBrush(colorset.Background);
        }

        return vm;
    }
}
