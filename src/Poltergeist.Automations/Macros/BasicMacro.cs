using System;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Macros;

public class BasicMacro : MacroBase
{
    public bool ShowStatus { get; set; } = true;

    public Action<BasicMacroScriptArguments> Script;

    public BasicMacro(string name) : base(name)
    {
    }

    protected internal override void OnConfigure(MacroServiceCollection services, IConfigureProcessor processor)
    {
        base.OnConfigure(services, processor);

        services.AddTransient<BasicMacroScriptArguments>();
    }

    protected internal override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var work = processor.GetService<WorkingService>();
        work.WorkProc = () =>
        {
            var args = processor.GetService<BasicMacroScriptArguments>();
            Script(args);
            return EndReason.Complete;
        };

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

        // todo: improve hooks to support args.message
        var hooks = processor.GetService<HookService>();
        hooks.Register("process_starting", _ =>
        {
            li.Update(0, new(ProgressStatus.Succeeded, "Initialized"));
        });
        hooks.Register("process_started", _ =>
        {
            li.Update(0, new(ProgressStatus.Busy, "Running"));
        });
        hooks.Register("process_exiting", _ =>
        {
            li.Update(0, new(ProgressStatus.Succeeded, "Completed"));
        });
        hooks.Register("error_occured", _ =>
        {
            li.Update(0, new(ProgressStatus.Failed, "Error"));
        });
    }
}
