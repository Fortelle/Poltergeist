using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Components.Panels;

public class PanelModel
{
    public string Key { get; }

    public string Header { get; }

    public bool ToLeft { get; init; }

    public ObservableCollection<IInstrumentModel> Instruments { get; } = new();

    public bool IsFilled { get; init; }

    public PanelModel(string key, string header)
    {
        Key = key;
        Header = header;
    }

    public PanelModel(string key, string header, params IInstrumentModel[] instruments) : this(key, header)
    {
        foreach(var instrument in instruments)
        {
            Instruments.Add(instrument);
        }
    }
}
