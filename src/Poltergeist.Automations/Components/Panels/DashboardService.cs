using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Panels;

public class DashboardService : MacroService
{
    private readonly PanelModel Panel;

    public bool IsEmpty => !Panel.Instruments.Any();

    public DashboardService(MacroProcessor processor, PanelService panelService) : base(processor)
    {
        Panel = panelService.Create(new("poltergeist-dashboard", LocalizationUtil.Localize("Dashboard_Header"))
        {
            ToLeft = true,
        });
    }

    public InstrumentModel Add(InstrumentModel instrument)
    {
        Panel.Instruments.Add(instrument);

        Logger.Trace($"Added an instrument '{instrument.GetType().Name}' to Dashboard panel.");

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

        Logger.Trace($"A new instrument '{instrument.GetType().Name}' is created.");

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

    public T GetOrCreate<T>(string key, Action<T>? config = null) where T : InstrumentModel
    {
        var instrument = Panel.Instruments.OfType<T>().FirstOrDefault(x => x.Key == key);
        if (instrument is not null)
        {
            return instrument;
        }

        instrument = Create(config);
        instrument.Key ??= key;
        return instrument;
    }

    public void Update<T>(string key, Action<T> action) where T : IInstrumentModel
    {
        var instrument = Get<T>(key);

        action(instrument);
    }

    public void Update<T>(Action<T> action) where T : IInstrumentModel
    {
        var instrument = Panel.Instruments.OfType<T>().FirstOrDefault();
        if (instrument is null)
        {
            throw new ArgumentException($"The panel type <{typeof(T)}> does not exist.");
        }

        action(instrument);
    }

}
