using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Components.Panels;

public interface IListInstrumentModel : IInstrumentModel
{
    ObservableCollection<ListInstrumentItem> Items { get; }
}
