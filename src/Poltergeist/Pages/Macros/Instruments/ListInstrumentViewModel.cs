using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Common.Structures.Colors;
using Poltergeist.Models;
using Poltergeist.Pages.Macros.Instruments;

namespace Poltergeist.Macros.Instruments;

public class ListInstrumentViewModel : IInstrumentViewModel
{
    public string? Title { get; set; }
    public SynchronizableCollection<ListInstrumentItem, ListInstrumentItemViewModel> Items { get; set; }

    public ListInstrumentViewModel(IListInstrumentModel model)
    {
        Title = model.Title;

        Items = new(model.Items, ModelToViewModel, App.MainWindow.DispatcherQueue);
    }

    private ListInstrumentItemViewModel? ModelToViewModel(ListInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new ListInstrumentItemViewModel(item);

        if(item.Color != null)
        {
            var colorset = ThemeColors.Colors[item.Color.Value];
            vm.Foreground = new SolidColorBrush(colorset.Foreground);
            vm.Background = new SolidColorBrush(colorset.Background);
        }

        return vm;
    }

}
