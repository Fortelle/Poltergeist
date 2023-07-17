using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Components.Panels;

public interface IListInstrumentModel : IInstrumentModel
{
    public ObservableCollection<ListInstrumentItem> Items { get; }
}
