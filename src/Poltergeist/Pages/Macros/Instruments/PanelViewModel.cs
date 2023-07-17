using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Models;
using Poltergeist.Pages.Macros.Instruments;

namespace Poltergeist.Macros.Instruments;

public class PanelViewModel
{
    public required string Key { get; init; }

    public required string Header { get; init; }

    public required InstrumentsWrapper Instruments { get; init; }
}

// encapsulate it for template selector
public class InstrumentsWrapper : SynchronizableCollection<IInstrumentModel, IInstrumentViewModel>
{
    public bool IsFilled { get; set; }

    public InstrumentsWrapper(ObservableCollection<IInstrumentModel> modelCollection, Func<IInstrumentModel?, IInstrumentViewModel?> func, DispatcherQueue dispatcherQueue) : base(modelCollection, func, dispatcherQueue)
    {

    }

    public IInstrumentViewModel? FirstItem => this.FirstOrDefault();
}
