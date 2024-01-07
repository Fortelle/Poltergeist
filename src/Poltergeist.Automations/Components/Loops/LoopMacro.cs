using Microsoft.Extensions.DependencyInjection;
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
    public Action<LoopExecuteArguments>? Execute;
    public Action<LoopCheckContinueArguments>? CheckContinue;
    public Action<ArgumentService>? After;

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

        if (Before != null)
        {
            loopService.Before += Before;
        }

        if (Execute != null)
        {
            loopService.Execute += Execute;
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
