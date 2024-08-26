using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Models;

namespace Poltergeist.Pages.Macros.Instruments;

// encapsulate it for template selector
public class InstrumentsWrapper : SynchronizableCollection<IInstrumentModel, IInstrumentViewModel>
{
    public bool IsFilled { get; set; }

    public IInstrumentViewModel? FirstItem => this.FirstOrDefault();

    public InstrumentsWrapper(ObservableCollection<IInstrumentModel> modelCollection, Func<IInstrumentModel?, IInstrumentViewModel?> func, DispatcherQueue dispatcherQueue) : base(modelCollection, func, dispatcherQueue)
    {
    }
}
