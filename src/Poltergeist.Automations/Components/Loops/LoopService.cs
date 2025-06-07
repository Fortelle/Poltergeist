using Microsoft.Extensions.Options;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Loops;

// Set Count or Duration to default to make the loop continue without checking the limitation of count or duration.
// If both are set to default, the loop will only iterate once.
// To enable infinite looping, set LoopOptions.IsInfiniteLoopable = true.

public class LoopService : MacroService
{
    public const string ConfigEnableKey = "loop-enable";
    public const string ConfigCountKey = "loop-count";
    public const string ConfigDurationKey = "loop-duration";
    public const string StatisticTotalIterationCountKey = "loop-totalcount";

    public int MaxCount { get; private set; }
    public TimeSpan MaxDuration { get; private set; }

    private readonly LoopOptions Options;
    private readonly HookService Hooks;
    private LoopResult Result;
    private int TotalIterations;

    public LoopService(MacroProcessor processor,
        HookService hooks,
        IOptions<LoopOptions> options
        ) : base(processor)
    {
        Hooks = hooks;
        Options = options.Value;

        processor.AddStep(new("loop-initialization", DoInitialization)
        {
            SuccessStepId = "loop-start",
            ErrorStepId = "loop-finalization",
            IsDefault = true,
        });

        processor.AddStep(new("loop-start", DoStart)
        {
            SuccessStepId = "loop-iteration",
            FailureStepId = "loop-end",
        });

        processor.AddStep(new("loop-iteration", DoIteration)
        {
            SuccessStepId = "loop-continue",
            ErrorStepId = "loop-finalization",
            IsInterruptable = true,
            Finally = DoIterationFinally,
        });

        processor.AddStep(new("loop-continue", DoCheckContinue)
        {
            SuccessStepId = "loop-iteration",
            FailureStepId = "loop-end",
            ErrorStepId = "loop-finalization",
        });

        processor.AddStep(new("loop-end", DoEnd)
        {
            SuccessStepId = "loop-finalization",
        });

        processor.AddStep(new("loop-finalization", DoFinalization)
        {
        });

    }

    private bool DoInitialization(WorkflowStepArguments args)
    {
        CheckLimit();

        switch (Options.Instrument)
        {
            case LoopInstrumentType.Grid:
                InstallProgressGrid();
                break;
            case LoopInstrumentType.ProgressBar:
                InstallProgressBar();
                break;
            case LoopInstrumentType.List:
                InstallProgressList();
                break;
        }

        Hooks.Raise<LoopInitializedHook>();

        return true;
    }

    private void CheckLimit()
    {
        var isLoopEnabled = Processor.Options.GetValueOrDefault<bool>(ConfigEnableKey);
        if (!isLoopEnabled)
        {
            MaxCount = 1;
            return;
        }

        if (Options.IsCountLimitable == true)
        {
            MaxCount = Processor.Options.GetValueOrDefault<int>(ConfigCountKey);
        }
        if (Options.IsDurationLimitable == true)
        {
            var duration = Processor.Options.GetValueOrDefault<TimeOnly>(ConfigDurationKey);
            MaxDuration = duration.ToTimeSpan();
        }

        if (MaxCount == default && MaxDuration == default && !Options.IsInfiniteLoopable)
        {
            MaxCount = 1;
            Logger.Warn("Neither max run count nor max run duration is defined. Since the macro does not support infinite repetition, the macro will only run once.");
        }
    }

    private bool DoStart(WorkflowStepArguments stepArguments)
    {
        var startHook = new LoopStartingHook();
        Hooks.Raise(startHook);
        if (startHook.Cancel)
        {
            Processor.Comment = startHook.CancelReason;
            Result = LoopResult.Unstarted;
            return false;
        }

        var afterHook = new LoopStartedHook()
        {
            IsCancelled = startHook.Cancel,
            CancelReason = startHook.CancelReason,
        };
        Hooks.Raise(afterHook);

        return true;
    }

    private void DoIteration(WorkflowStepArguments stepArguments)
    {
        var iterationIndex = (stepArguments.PreviousResult?.Output as int?) ?? 0;
        var iterationData = new IterationData()
        {
            Index = iterationIndex,
        };

        stepArguments.Output = iterationData;

        Logger.Info($"Loop: {iterationIndex + 1}");

        var beginHook = new IterationStartingHook()
        {
            Index = iterationIndex,
            StartTime = stepArguments.StartTime,
        };
        Hooks.Raise(beginHook);

        var iterationHook = new IterationExecutingHook()
        {
            Index = iterationIndex,
            Result = IterationResult.Undetermined,
        };
        Hooks.Raise(iterationHook);

        iterationData.Result = iterationHook.Result == IterationResult.Undetermined ? IterationResult.Continue : iterationHook.Result;
    }

    private void DoIterationFinally(WorkflowStepFinallyArguments stepArguments)
    {
        if (stepArguments.Output is not IterationData data)
        {
            throw new InvalidOperationException();
        }

        TotalIterations += 1;

        var endHook = new IterationEndHook()
        {
            Index = data.Index,
            StartTime = stepArguments.StartTime,
            EndTime = stepArguments.EndTime,
            Duration = stepArguments.Duration,
            Result = stepArguments.Result switch
            {
                WorkflowStepResult.Success => IterationResult.Continue,
                WorkflowStepResult.Failed => IterationResult.Failed,
                WorkflowStepResult.Error => IterationResult.Error,
                WorkflowStepResult.Interrupted => IterationResult.Interrupted,
                _ => data.Result,
            },
        };
        Hooks.Raise(endHook);
    }

    private bool DoCheckContinue(WorkflowStepArguments stepArguments)
    {
        if (stepArguments.PreviousResult is null)
        {
            throw new InvalidOperationException();
        }
        if (stepArguments.PreviousResult.Output is not IterationData prevIterationData)
        {
            throw new InvalidOperationException();
        }

        var nextIterationIndex = -1;

        if (prevIterationData.Result == IterationResult.Break)
        {
            Result = LoopResult.Broken;
            Logger.Debug($"The loop will be stopped because the previous iteration has returned {prevIterationData.Result}.");
        }
        else if (stepArguments.PreviousResult.Result == WorkflowStepResult.Failed)
        {
            Result = LoopResult.Broken;
            Logger.Debug($"The loop will be stopped because the previous iteration has failed.");
        }
        else if (stepArguments.PreviousResult.Result == WorkflowStepResult.Error)
        {
            Result = LoopResult.Error;
            Logger.Debug($"The loop will be stopped because an error has occurred in the previous iteration.");
        }
        else if (stepArguments.PreviousResult.Result == WorkflowStepResult.Interrupted)
        {
            Result = LoopResult.Interrupted;
            Logger.Debug($"The loop will be stopped due to user interruption.");
        }
        else if (Processor.Status == ProcessorStatus.Stopping)
        {
            Result = LoopResult.Interrupted;
            Logger.Debug($"The loop will be stopped because the processor is stopping.");
        }
        else if (CheckTimeout())
        {
        }
        else if (prevIterationData.Result == IterationResult.RestartLoop)
        {
            nextIterationIndex = 0;
            Logger.Debug($"The loop will be restarted because the previous iteration has returned {prevIterationData.Result}.");
        }
        else if (prevIterationData.Result == IterationResult.RestartIteration)
        {
            nextIterationIndex = prevIterationData.Index;
            Logger.Debug($"The iteration will be restarted because the previous iteration has returned {prevIterationData.Result}.");
        }
        else if (CheckCount())
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the specified count.", new { prevIterationData.Index, MaxCount });
        }
        else if (Options.MaxIterationLimit > 0 && prevIterationData.Index >= Options.MaxIterationLimit - 1)
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the max iteration limit.", new { prevIterationData.Index, Options.MaxIterationLimit });
        }
        else if (prevIterationData.Result == IterationResult.ForceContinue)
        {
            nextIterationIndex = prevIterationData.Index + 1;
            Logger.Debug($"The loop will be continued because the previous iteration has returned {prevIterationData.Result}.");
        }
        else
        {
            var result = CheckContinue();
            switch (result)
            {
                case CheckContinueResult.Break:
                    Result = LoopResult.Broken;
                    Logger.Debug($"The loop will be stopped because the continue check returns {result}.");
                    break;
                case CheckContinueResult.RestartLoop:
                    nextIterationIndex = 0;
                    Logger.Debug($"The loop will be restarted because the continue check returns {result}.");
                    break;
                case CheckContinueResult.RestartIteration:
                    nextIterationIndex = prevIterationData.Index;
                    Logger.Debug($"The iteration will be restarted because the continue check returns {result}.");
                    break;
                default:
                    nextIterationIndex = prevIterationData.Index + 1;
                    Logger.Debug($"The loop will be continued because the continue check returns {result}.");
                    break;
            }
        }

        Logger.Trace(nextIterationIndex);

        if (nextIterationIndex == -1)
        {
            return false; // goto after
        }

        stepArguments.Output = nextIterationIndex;

        return true; // goto iteration

        bool CheckCount()
        {
            if (MaxCount == 0)
            {
                return false;
            }

            if (prevIterationData.Index >= MaxCount - 1)
            {
                return true;
            }

            return false;
        }

        bool CheckTimeout()
        {
            if (MaxDuration == default)
            {
                return false;
            }

            var duration = Processor.GetElapsedTime();
            if (duration > MaxDuration)
            {
                Result = LoopResult.Complete;
                Logger.Debug($"The loop will be stopped because the run duration has reached the specified time.", new { Processor.StartTime, duration, MaxDuration });
                return true;
            }

            return false;
        }

        CheckContinueResult CheckContinue()
        {
            var checkContinueHook = new LoopCheckContinueHook()
            {
                Data = prevIterationData,
                Result = CheckContinueResult.Continue,
            };
            Processor.GetService<HookService>().Raise(checkContinueHook);

            return checkContinueHook.Result;
        }
    }

    private void DoEnd(WorkflowStepArguments stepArguments)
    {
        var beginHook = new LoopEndingHook()
        {
            Result = Result,
            TotalIterations = TotalIterations,
            Comment = Processor.Comment ?? ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Comment", TotalIterations),
        };
        Hooks.Raise(beginHook);

        Processor.Comment ??= beginHook.Comment;

        Processor.Statistics.AddOrUpdate(StatisticTotalIterationCountKey, TotalIterations, x => x + TotalIterations);
        Processor.OutputStorage.TryAdd(StatisticTotalIterationCountKey, TotalIterations);

        var endHook = new LoopEndedHook()
        {
            Result = Result,
            TotalIterations = TotalIterations,
            Comment = beginHook.Comment,
        };
        Hooks.Raise(endHook);
    }

    private void DoFinalization()
    {
        Hooks.Raise<LoopFinalizedHook>();
    }

    private void InstallProgressGrid()
    {
        var dashboard = Processor.GetService<DashboardService>();

        var gridInstrument = dashboard.Create<ProgressGridInstrument>(instrument =>
        {
            instrument.Title = Options.Title ?? ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_Title");
            instrument.IsSticky = true;
        });

        Hooks.Register<LoopInitializedHook>(e =>
        {
            if (MaxCount > 0)
            {
                for (var i = 0; i < MaxCount; i++)
                {
                    var info = GetInfo() ?? new ProgressInstrumentInfo()
                    {
                        Status = ProgressStatus.Idle,
                    };
                    gridInstrument.Add(new(info));
                }
            }
        });

        Hooks.Register<UpdatetInstrumentInfoHook>(e =>
        {
            if (e.Info == null)
            {
                return;
            }
            gridInstrument.Update(e.Index, new(e.Info));
        });

        Hooks.Register<IterationStartingHook>(e =>
        {
            gridInstrument.Update(e.Index, new(ProgressStatus.Busy));
        });

        Hooks.Register<IterationEndHook>(e =>
        {
            var status = e.Result switch
            {
                IterationResult.Failed => ProgressStatus.Failure,
                IterationResult.Error => ProgressStatus.Failure,
                IterationResult.Interrupted => ProgressStatus.Warning,
                _ => ProgressStatus.Success,
            };
            gridInstrument.Update(e.Index, new(status));
        });
    }

    private void InstallProgressBar()
    {
        var dashboard = Processor.GetService<DashboardService>();

        var listInstrument = dashboard.Create<ProgressListInstrument>(instrument =>
        {
            instrument.Title = Options.Title ?? ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_Title");
            instrument.IsSticky = true;
        });

        Hooks.Register<LoopInitializedHook>(e =>
        {
            listInstrument.Add(new(ProgressStatus.Busy)
            {
                Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_Text"),
                Progress = MaxCount <= 0 ? 1 : 0,
                Subtext = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_Subtext"),
            });
        });

        Hooks.Register<IterationStartingHook>(e =>
        {
            if (MaxCount <= 0)
            {
                listInstrument.Update(0, new()
                {
                    Subtext = $"{e.Index + 1}",
                });
            }
            else
            {
                listInstrument.Update(0, new()
                {
                    Progress = (e.Index + 1d) / (MaxCount + 1),
                    Subtext = $"{e.Index + 1} / {MaxCount}",
                });
            }
        });

        Hooks.Register<UpdatetInstrumentInfoHook>(e =>
        {
            if (e.Info == null)
            {
                return;
            }
            listInstrument.Update(0, new(e.Info));
        });

        Hooks.Register<LoopEndedHook>(e =>
        {
            var status = e.Result switch
            {
                LoopResult.Unstarted => ProgressStatus.Warning,
                LoopResult.Error => ProgressStatus.Failure,
                LoopResult.Interrupted => ProgressStatus.Warning,
                _ => ProgressStatus.Success,
            };
            listInstrument.Update(0, new(status)
            {
                Subtext = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_SuccessText"),
                Progress = 1,
            });
        });
    }

    private void InstallProgressList()
    {
        var dashboard = Processor.GetService<DashboardService>();

        var listInstrument = dashboard.Create<ProgressListInstrument>(instrument =>
        {
            instrument.Title = Options.Title ?? ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_Title");
        });

        Hooks.Register<LoopInitializedHook>(e =>
        {
            if (MaxCount > 0)
            {
                for (var i = 0; i < MaxCount; i++)
                {
                    var info = GetInfo() ?? new ProgressInstrumentInfo()
                    {
                        Status = ProgressStatus.Idle,
                        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", i + 1)
                    };
                    listInstrument.Add(new(info));
                }
            }
        });

        Hooks.Register<IterationStartingHook>(e =>
        {
            listInstrument.Update(e.Index, new(ProgressStatus.Busy)
            {
                Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", e.Index + 1),
            });
        });

        Hooks.Register<UpdatetInstrumentInfoHook>(e =>
        {
            if (e.Info == null)
            {
                return;
            }
            listInstrument.Update(e.Index, new(e.Info!));
        });

        Hooks.Register<IterationEndHook>(e =>
        {
            var status = e.Result switch
            {
                IterationResult.Failed => ProgressStatus.Failure,
                IterationResult.Error => ProgressStatus.Failure,
                IterationResult.Interrupted => ProgressStatus.Warning,
                _ => ProgressStatus.Success,
            };
            listInstrument.Update(e.Index, new(status));
        });
    }

    private ProgressInstrumentInfo? GetInfo()
    {
        var hook = new GetInitialInfoHook();
        Hooks.Raise(hook);
        return hook.Info;
    }

}
