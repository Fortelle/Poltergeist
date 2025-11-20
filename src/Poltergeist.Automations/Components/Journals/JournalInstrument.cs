using System.Collections.ObjectModel;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Journals;

public class JournalInstrument : InstrumentModel
{
    public ObservableCollection<string> Lines { get; } = new();

    public JournalInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(string text)
    {
        Lines.Add(text);
    }
}
