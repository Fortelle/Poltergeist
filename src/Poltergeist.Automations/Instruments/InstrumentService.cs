using System;
using System.Linq;
using Poltergeist.Automations.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Instruments;

public class InstrumentService : MacroService
{
    private InstrumentPanel Panel;

    public InstrumentService(MacroProcessor processor) : base(processor)
    {
        processor.Hooks.Register("ui_panel_ready", UpdateUI);
    }

    private void UpdateUI(object[] args)
    {
        var panelService = Processor.GetService<PanelService>();
        Panel = panelService.Create<InstrumentPanel>(panel =>
        {
            panel.Key = "poltergeist-instruments";
            panel.Header = "Dashboard";
        });
    }

    public T Create<T>(Action<T> config) where T : class, IInstrumentModel
    {
        var instrument = Processor.GetService<T>();
        config(instrument);

        Panel.Add(instrument);

        instrument.IsCreated = true;

        Log(Logging.LogLevel.Debug, $"Created a new instrument <{instrument.GetType().Name}>(\"{instrument.Key}\").");

        return instrument;
    }

    public void Update<T>(string key, Action<T> action) where T : IInstrumentModel
    {
        var instrument = Panel.Get<T>(key);
        action(instrument);
    }

    public void Update<T>(Action<T> action) where T : IInstrumentModel
    {
        var instrument = Panel.Get<T>();
        action(instrument);
    }

}
