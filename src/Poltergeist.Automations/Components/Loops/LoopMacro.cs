using System;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Repetitions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Components.Loops;

namespace Poltergeist.Automations.Components.Loops;

public class LoopMacro : MacroBase
{
    public LoopOptions LoopOptions { get; } = new()
    {
        IsCountLimitable = true,
        IsDurationLimitable = true,
        Instrument = LoopInstrumentType.List,
    };

    public Action<LoopBeforeArguments>? Before;
    public Action<LoopExecutionArguments>? Execution;
    public Action<LoopCheckContinueArguments>? CheckContinue;
    public Action<ArgumentService>? After;

    public LoopMacro(string name) : base(name)
    {
    }

    protected override void OnInitialize()
    {
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new CompleteModule());
    }

    protected override void OnConfigure(ServiceCollection services, IConfigureProcessor processor)
    {
        base.OnConfigure(services, processor);

        services.Configure<LoopOptions>(options =>
        {
            options.Instrument = LoopInstrumentType.List;
        });
    }

    protected override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var loopService = processor.GetService<LoopService>();

        if (Before != null)
        {
            loopService.Before += Before;
        }

        if (Execution != null)
        {
            loopService.Execution += Execution;
        }

        if (CheckContinue != null)
        {
            loopService.CheckContinue += CheckContinue;
        }

        if (After != null)
        {
            loopService.After += After;
        }
    }

}
