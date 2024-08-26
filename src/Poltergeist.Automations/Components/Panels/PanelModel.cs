using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Components.Panels;

public class PanelModel
{
    public string Key { get; }

    public string Header { get; set; }

    public bool ToLeft { get; set; }

    public bool IsFilled { get; set; }

    public ObservableCollection<IInstrumentModel> Instruments { get; } = new();

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
