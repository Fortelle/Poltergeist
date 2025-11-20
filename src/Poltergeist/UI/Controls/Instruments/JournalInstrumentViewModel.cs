using Poltergeist.Automations.Components.Journals;
using Poltergeist.Models;

namespace Poltergeist.UI.Controls.Instruments;

public class JournalInstrumentViewModel : IInstrumentViewModel
{
    public string? Title { get; set; }

    public SynchronizableCollection<string, string> Items { get; set; }

    public JournalInstrumentViewModel(JournalInstrument model)
    {
        Title = model.Title;

        Items = new(model.Lines, ModelToViewModel, PoltergeistApplication.Current.DispatcherQueue);
    }

    private string? ModelToViewModel(string? text)
    {
        return text;
    }

}
