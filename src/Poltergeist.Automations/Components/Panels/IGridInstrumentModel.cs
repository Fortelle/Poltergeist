using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Components.Panels;

public interface IGridInstrumentModel : IInstrumentModel
{
    public int? MaximumColumns { get; }
    public int? IconSize { get; }
    public int? IconWidth { get; }
    public int? IconHeight { get; }
    public ObservableCollection<GridInstrumentItem> Items { get; }
}
