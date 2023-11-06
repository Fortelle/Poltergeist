using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Panels;

public abstract class InstrumentModel : MacroService, IInstrumentModel
{
    public string? Key { get; set; }
    public string? Title { get; set; }
    public string MacroKey { get; set; }
    public string ProcessId { get; set; }

    public bool AutoScroll { get; set; }
    public bool Stretch { get; set; }

    public bool IsSticky { get; set; }

    public InstrumentModel(MacroProcessor processor) : base(processor)
    {
        MacroKey = processor.Macro.Name;
        ProcessId = processor.ProcessId;
    }
}
