using Microsoft.Extensions.Options;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Loops;

// Set Count or Duration to default to make the loop continue without checking the limitation of count or duration.
// If both are set to default, the loop will only iterate once.
// To enable infinite looping, set LoopOptions.IsInfiniteLoopable = true.

public class LoopService : LoopBuilderService, IAutoloadable
{
    public const string ConfigEnableKey = "loop-enable";
    public const string ConfigCountKey = "loop-count";
    public const string ConfigDurationKey = "loop-duration";
    public const string StatisticTotalIterationCountKey = "loop-totaliterationcount";

    public Action<LoopBeforeArguments>? Before;
    public Action<ArgumentService>? After;

    private readonly LoopOptions Options;
    private readonly HookService Hooks;

    public LoopService(MacroProcessor processor,
        HookService hooks,
        IOptions<LoopOptions> options
        ) : base(processor)
    {
        Hooks = hooks;
        Options = options.Value;

        ContinuesOnError = Options.ContinuesOnError;
        InstrumentType = Options.Instrument;
        MaxIterationLimit = Options.MaxIterationLimit;
        ExcludesIncompleteIteration = Options.ExcludesIncompleteIteration;
        IsSticky = true;

        processor.WorkProc = WorkProc;
        Hooks.Register<ProcessorEndingHook>(OnProcessorEnding);
    }

    private void WorkProc()
    {
        CheckLimit();

        if (!DoBefore())
        {
            return;
        }

        Run();

        if (Exception is not null)
        {
            throw Exception;
        }

        DoAfter();
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
            var duration = Processor.Options.Get<TimeOnly>(ConfigDurationKey);
            MaxDuration = duration.ToTimeSpan();
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

        Hooks.Raise(new LoopEndedHook());

        Logger.Debug($"Finished running the after-loop procedure.");
    }

    private void OnProcessorEnding(ProcessorEndingHook hook, IUserProcessor processor)
    {
        hook.Comment ??= ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Comment", IterationCount);
        processor.Statistics.Set<int>(StatisticTotalIterationCountKey, x => x + IterationCount);
    }

}
