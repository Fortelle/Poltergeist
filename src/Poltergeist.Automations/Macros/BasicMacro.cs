using System;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Macros;

public class BasicMacro : MacroBase
{
    public bool ShowStatus { get; set; } = true;

    public Action<MacroProcessor> Script;

    public BasicMacro(string name) : base(name)
    {
    }

    protected internal override void ConfigureProc(MacroServiceCollection services)
    {
        base.ConfigureProc(services);
    }

    protected internal override void ReadyProc(MacroProcessor processor)
    {
        base.ReadyProc(processor);

        var work = processor.GetService<WorkingService>();
        work.WorkProc = () => Script(processor);

        SetStatus(processor);
    }

    private void SetStatus(MacroProcessor processor)
    {
        if (!ShowStatus) return;

        var ns = processor.GetService<InstrumentService>();
        var li = ns.Create<ListInstrument>(inst =>
        {
            inst.Key = "basic_instrument";
            inst.Title = "Status:";
        });

        var hooks = processor.GetService<HookService>();
        hooks.Register("process_starting", _ =>
        {
            li.Update(0, new(ProgressStatus.Succeeded, "Initialized"));
        });
        hooks.Register("process_started", _ =>
        {
            li.Update(0, new(ProgressStatus.Busy, "Running"));
        });
        hooks.Register("process_ended", _ =>
        {
            li.Update(0, new(ProgressStatus.Succeeded, "Completed"));
        });
        hooks.Register("error_occured", _ =>
        {
            li.Update(0, new(ProgressStatus.Failed, "Error"));
        });
    }
}
