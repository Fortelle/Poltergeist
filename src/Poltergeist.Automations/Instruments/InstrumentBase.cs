using System.Windows.Controls;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Instruments;

public abstract class InstrumentModel<TView, TViewModel> : MacroService, IInstrumentModel
    where TView : UserControl, new()
{
    public string Key { get; set; }
    public string Title { get; set; }
    public bool IsCreated { get; set; }

    public InstrumentModel(MacroProcessor processor) : base(processor)
    {
    }

    public virtual UserControl CreateControl()
    {
        return new TView();
    }

    //public virtual object CreateViewModel()
    //{
    //    return new TView();
    //}
    public abstract InstrumentItemViewModel CreateViewModel();
}

public interface IInstrumentModel
{
    public string Key { get; set; }
    public string Title { get; set; }
    public bool IsCreated { get; set; }
    public UserControl CreateControl();
    public InstrumentItemViewModel CreateViewModel();
}
