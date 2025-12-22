using Poltergeist.Automations.Components.Panels;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class LabelInstrumentViewModel : IInstrumentViewModel
{
    public string? Title { get; set; }
    public int MaximumColumns { get; set; }

    public SynchronizableCollection<LabelInstrumentItem, LabelInstrumentItemViewModel> Items { get; set; }

    public LabelInstrumentViewModel(LabelInstrument model)
    {
        Title = model.Title;
        MaximumColumns = model.MaximumColumns ?? -1;

        Items = new(model.Items, ModelToViewModel, PoltergeistApplication.Current.DispatcherQueue);
    }

    private LabelInstrumentItemViewModel? ModelToViewModel(LabelInstrumentItem? item)
    {
        if (item is null)
        {
            return null;
        }

        var vm = new LabelInstrumentItemViewModel(item);

        return vm;
    }
}
