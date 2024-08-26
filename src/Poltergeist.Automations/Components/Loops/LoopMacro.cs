using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

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
    public Action<LoopExecuteArguments>? Execute;
    public Action<LoopCheckContinueArguments>? CheckContinue;
    public Action<ArgumentService>? After;
    public Func<int, ProgressInstrumentInfo>? InitializeInfo;

    public LoopMacro(string name) : base(name)
    {
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new CompleteModule());
    }

    protected override void OnConfigure(IConfigurableProcessor processor)
    {
        base.OnConfigure(processor);

        processor.Services.Configure<LoopOptions>(options =>
        {
            options.Instrument = LoopInstrumentType.List;
        });
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        var loopService = processor.GetService<LoopService>();

        if (Before is not null)
        {
            loopService.Before += Before;
        }

        if (Execute is not null)
        {
            loopService.Execute += Execute;
        }

        if (CheckContinue is not null)
        {
            loopService.CheckContinue += CheckContinue;
        }

        if (After is not null)
        {
            loopService.After += After;
        }

        if (InitializeInfo is not null)
        {
            loopService.InitializeInfo += InitializeInfo;
        }
    }

}
