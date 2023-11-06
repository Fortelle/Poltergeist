using System;
using System.Linq;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Panels;

public class DashboardService : MacroService
{
    private readonly PanelModel Panel;

    public DashboardService(MacroProcessor processor, PanelService panelService) : base(processor)
    {
        Panel = panelService.Create(new("poltergeist-dashboard", "Dashboard")
        {
            ToLeft = true,
        });
    }

    public InstrumentModel Add(InstrumentModel instrument)
    {
        Panel.Instruments.Add(instrument);

        Logger.Debug($"Added an instrument <{instrument.GetType().Name}> to Dashboard panel.");

        return instrument;
    }

    public T Create<T>(Action<T>? config = null) where T : InstrumentModel
    {
        var instrument = Processor.GetService<T>();
        config?.Invoke(instrument);

        if (instrument.IsSticky)
        {
            Panel.Instruments.Insert(0, instrument);
        }
        else
        {
            Panel.Instruments.Add(instrument);
        }

        Logger.Debug($"A new instrument <{instrument.GetType().Name}> is created.");

        return instrument;
    }

    public T Get<T>(string key) where T : IInstrumentModel
    {
        var instrument = Panel.Instruments.OfType<T>().FirstOrDefault(x => x.Key == key);
        if (instrument is null)
        {
            throw new ArgumentException($"The panel key '{key}' does not exist.");
        }

        return instrument;
    }

    public void Update<T>(string key, Action<T> action) where T : IInstrumentModel
    {
        var instrument = Get<T>(key);

        Processor.RaiseAction(() =>
        {
            action(instrument);
        });
    }

    public void Update<T>(Action<T> action) where T : IInstrumentModel
    {
        var instrument = Panel.Instruments.OfType<T>().FirstOrDefault();
        if(instrument is null)
        {
            throw new ArgumentException($"The panel type <{typeof(T)}> does not exist.");
        }

        Processor.RaiseAction(() =>
        {
            action(instrument);
        });
    }

}
