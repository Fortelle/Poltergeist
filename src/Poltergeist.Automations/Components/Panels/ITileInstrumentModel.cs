using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Components.Panels;

public interface ITileInstrumentModel : IInstrumentModel
{
    int? IconSize { get; }
    int? IconWidth { get; }
    int? IconHeight { get; }
    ObservableCollection<TileInstrumentItem> Items { get; }
}
