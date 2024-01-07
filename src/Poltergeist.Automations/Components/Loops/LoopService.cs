using Microsoft.Extensions.Options;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Loops;

// Set Count or Duration to default to make the loop continue without checking the limitation of count or duration.
// If both are set to default, the loop will only iterate once.
// To enable infinite looping, set LoopOptions.IsInfiniteLoopable = true.

public class LoopService : MacroService, IAutoloadable
{
    public const string ConfigEnableKey = "loop-enable";
    public const string ConfigCountKey = "loop-count";
    public const string ConfigDurationKey = "loop-duration";
    public const string StatisticTotalIterationCountKey = "loop-totaliterationcount";

    public Action<LoopBeforeArguments>? Before { get; set; }
    public Action<LoopExecuteArguments>? Execute { get; set; }
    public Action<LoopCheckContinueArguments>? CheckContinue { get; set; }
    public Action<ArgumentService>? After { get; set; }

    private int MaxCount;
    private TimeSpan MaxDuration;

    private readonly LoopOptions Options;
    private readonly HookService Hooks;
    private readonly LoopBuilderService LoopHelper;

    private EndReason Status = EndReason.None;

    public LoopService(MacroProcessor processor,
        HookService hooks,
        IOptions<LoopOptions> options,
        LoopBuilderService loophelper
        ) : base(processor)
    {
        Hooks = hooks;
        Options = options.Value;
        LoopHelper = loophelper;

        processor.WorkProc = WorkProc;
    }

    private void WorkProc()
    {
        CheckLimit();

        if (!DoBefore())
        {
            return;
        }

        LoopHelper.MaxCount = MaxCount;
        LoopHelper.MaxDuration = MaxDuration;
        LoopHelper.Execute = Execute;
        LoopHelper.CheckContinue = CheckContinue;
        LoopHelper.InstrumentType = Options.Instrument;
        LoopHelper.MaxIterationLimit = Options.MaxIterationLimit;
        LoopHelper.IsSticky = true;

        LoopHelper.Run();

        if(LoopHelper.Exception is not null)
        {
            throw LoopHelper.Exception;
        }

        DoAfter();

        if (Status == EndReason.None)
        {
            Status = EndReason.Complete;
        }
    }

    private void CheckLimit()
    {
        var usesLoop = Processor.Options.Get<bool>(ConfigEnableKey);
        if (!usesLoop)
        {
            MaxCount = 1;
            return;
        }

        if (Options.IsCountLimitable == true)
        {
            MaxCount = Processor.Options.Get<int>(ConfigCountKey);
        }
        if (Options.IsDurationLimitable == true)
        {
            var seconds = Processor.Options.Get<int>(ConfigDurationKey);
            MaxDuration = TimeSpan.FromSeconds(seconds);
        }

        if (MaxCount == default && MaxDuration == default && !Options.IsInfiniteLoopable)
        {
            MaxCount = 1;
            Logger.Warn($"Neither '{ConfigCountKey}' nor '{ConfigDurationKey}' is set. To enable infinite looping, set {nameof(LoopOptions)}.{nameof(LoopOptions.IsInfiniteLoopable)} to true.");
        }
    }

    private bool DoBefore()
    {
        Logger.Debug("Started running the before-loop procedure.");

        var canBegin = true;

        if (Before is not null)
        {
            var args = Processor.GetService<LoopBeforeArguments>();
            args.CountLimit = MaxCount;
            Before.Invoke(args);
            if (args.Cancel)
            {
                canBegin = false;
            }
            MaxCount = args.CountLimit;
            Processor.Comment = args.Comment;
        }

        Hooks.Raise(new LoopStartedHook());

        Logger.Debug("Finished running the before-loop procedure.", new { canBegin });

        return canBegin;
    }

    private void DoAfter()
    {
        Logger.Debug($"Started running the after-loop procedure.");

        if (After is not null)
        {
            var args = Processor.GetService<ArgumentService>();
            After.Invoke(args);
        }

        var iterationCount = LoopHelper.IterationIndex + 1;
        Processor.Statistics.Set<int>(StatisticTotalIterationCountKey, x => x + iterationCount);

        Hooks.Raise(new LoopEndedHook());

        Logger.Debug($"Finished running the after-loop procedure.");
    }

}
