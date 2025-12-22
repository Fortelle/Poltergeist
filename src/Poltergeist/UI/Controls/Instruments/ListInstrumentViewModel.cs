using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Helpers;
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

        return vm;
    }
}
