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
    public const string StatisticTotalIterationCountKey = "loop_total_iterations";
    public const string ReportIterationCountKey = "loop_iterations";

    public int MaxCount { get; private set; }
    public TimeSpan MaxDuration { get; private set; }

    private readonly HookService Hooks;
    private LoopOptions Options = new();
    private LoopResult Result;
    private int TotalIterations;

    public LoopService(MacroProcessor processor,
        HookService hooks
        ) : base(processor)
    {
        Hooks = hooks;

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
            IsInterruptable = true,
            Finally = DoIterationFinally,
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
        Options = Processor.SessionStorage.GetValueOrDefault<LoopOptions>("loop-options") ?? Options;
        
        CheckLimit();

        switch (Options.Instrument)
        {
            case LoopInstrumentType.Tile:
                InstallProgressTile();
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
            Processor.Report.TryAdd("comment_message", startHook.CancelReason);
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
        };
        Hooks.Raise(iterationHook);

        var iterationData = new IterationData()
        {
            Index = iterationIndex,
            IsInvalid = iterationHook.IsInvalid != false,
            Result = iterationHook.Result,
        };
        stepArguments.Output = iterationData;
    }

    private void DoIterationFinally(WorkflowStepFinallyArguments stepArguments)
    {
        if (stepArguments.Output is not IterationData iterationData)
        {
            return;
        }

        if (!iterationData.IsInvalid)
        {
            TotalIterations += 1;
        }

        var result = stepArguments.Result switch
        {
            WorkflowStepResult.Success => IterationResult.Continue,
            WorkflowStepResult.Failed => IterationResult.Failed,
            WorkflowStepResult.Error => IterationResult.Error,
            WorkflowStepResult.Interrupted => IterationResult.Interrupted,
            _ => iterationData.Result,
        };

        var nextIterationIndex = CheckContinue(iterationData.Index, iterationData.Result);

        var endHook = new IterationEndHook()
        {
            Index = iterationData.Index,
            NextIndex = nextIterationIndex,
            StartTime = stepArguments.StartTime,
            EndTime = stepArguments.EndTime,
            Duration = stepArguments.Duration,
            Result = result,
        };
        Hooks.Raise(endHook);

        stepArguments.Output = nextIterationIndex;
        stepArguments.NextStepId = nextIterationIndex >= 0 ? "loop-iteration" : "loop-end";
    }

    private int CheckContinue(int iterationIndex, IterationResult iterationResult)
    {
        var nextIterationIndex = -1;

        if (iterationResult == IterationResult.Break)
        {
            Result = LoopResult.Broken;
            Logger.Debug($"The loop will be stopped because the previous iteration has returned {iterationResult}.");
        }
        else if (iterationResult == IterationResult.Failed)
        {
            Result = LoopResult.Broken;
            Logger.Debug($"The loop will be stopped because the previous iteration has failed.");
        }
        else if (iterationResult == IterationResult.Error)
        {
            Result = LoopResult.Error;
            Logger.Debug($"The loop will be stopped because an error has occurred in the previous iteration.");
        }
        else if (iterationResult == IterationResult.Interrupted)
        {
            Result = LoopResult.Interrupted;
            Logger.Debug($"The loop will be stopped due to user interruption.");
        }
        else if (Processor.Status == ProcessorStatus.Stopping)
        {
            Result = LoopResult.Interrupted;
            Logger.Debug($"The loop will be stopped because the processor is stopping.");
        }
        else if (CheckTimeout(out var duration))
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the run duration has reached the specified time.", new { duration, MaxDuration });
        }
        else if (iterationResult == IterationResult.RestartLoop)
        {
            nextIterationIndex = 0;
            Logger.Debug($"The loop will be restarted because the previous iteration has returned {iterationResult}.");
        }
        else if (iterationResult == IterationResult.RestartIteration)
        {
            nextIterationIndex = iterationIndex;
            Logger.Debug($"The iteration will be restarted because the previous iteration has returned {iterationResult}.");
        }
        else if (CheckCount())
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the specified count.", new { iterationIndex, MaxCount });
        }
        else if (Options.MaxIterationLimit > 0 && iterationIndex >= Options.MaxIterationLimit - 1)
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the max iteration limit.", new { iterationIndex, Options.MaxIterationLimit });
        }
        else if (iterationResult == IterationResult.ForceContinue)
        {
            nextIterationIndex = iterationIndex + 1;
            Logger.Debug($"The loop will be continued because the previous iteration has returned {iterationResult}.");
        }
        else
        {
            var checkResult = RaiseCheckContinueHook();
            switch (checkResult)
            {
                case CheckContinueResult.Break:
                    Result = LoopResult.Broken;
                    Logger.Debug($"The loop will be stopped because the continue check returns {checkResult}.");
                    break;
                case CheckContinueResult.RestartLoop:
                    nextIterationIndex = 0;
                    Logger.Debug($"The loop will be restarted because the continue check returns {checkResult}.");
                    break;
                case CheckContinueResult.RestartIteration:
                    nextIterationIndex = iterationIndex;
                    Logger.Debug($"The iteration will be restarted because the continue check returns {checkResult}.");
                    break;
                default:
                    nextIterationIndex = iterationIndex + 1;
                    Logger.Debug($"The loop will be continued because the continue check returns {checkResult}.");
                    break;
            }
        }

        return nextIterationIndex;

        bool CheckCount()
        {
            if (MaxCount == 0)
            {
                return false;
            }

            if (iterationIndex >= MaxCount - 1)
            {
                return true;
            }

            return false;
        }

        bool CheckTimeout(out TimeSpan duration)
        {
            if (MaxDuration == default)
            {
                duration = default;
                return false;
            }

            var elapsedTime = Processor.GetElapsedTime();
            if (elapsedTime > MaxDuration)
            {
                duration = elapsedTime;
                return true;
            }

            duration = default;
            return false;
        }

        CheckContinueResult RaiseCheckContinueHook()
        {
            var checkContinueHook = new LoopCheckContinueHook()
            {
                IterationIndex = iterationIndex,
                IterationResult = iterationResult,
                Result = CheckContinueResult.Continue,
            };
            Processor.GetService<HookService>().Raise(checkContinueHook);

            return checkContinueHook.Result;
        }
    }

    private void DoEnd(WorkflowStepArguments stepArguments)
    {
        Processor.Report.TryAdd("comment_message", LocalizationUtil.Localize("Loops_Comment", TotalIterations));

        if (Result == LoopResult.Unknown && stepArguments.PreviousResult?.Result == WorkflowStepResult.Error)
        {
            Result = LoopResult.Error;
        }
        else if (Result == LoopResult.Unknown && stepArguments.PreviousResult?.Result == WorkflowStepResult.Failed)
        {
            Result = LoopResult.Broken;
        }

        var endingHook = new LoopEndingHook()
        {
            Result = Result,
            TotalIterations = TotalIterations,
        };
        Hooks.Raise(endingHook);

        Hooks.Register<ProcessorEndingHook>(e =>
        {
            e.Report.Add(ReportIterationCountKey, TotalIterations);
        });

        var endHook = new LoopEndedHook()
        {
            Result = Result,
            TotalIterations = TotalIterations,
        };
        Hooks.Raise(endHook);
    }

    private void DoFinalization()
    {
        Hooks.Raise<LoopFinalizedHook>();
    }

    private void InstallProgressTile()
    {
        var dashboard = Processor.GetService<DashboardService>();

        var gridInstrument = dashboard.Create<ProgressTileInstrument>(instrument =>
        {
            instrument.Title = Options.Title ?? LocalizationUtil.Localize("Loops_Instrument_Title");
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
            instrument.Title = Options.Title ?? LocalizationUtil.Localize("Loops_Instrument_Title");
            instrument.IsSticky = true;
        });

        Hooks.Register<LoopInitializedHook>(e =>
        {
            listInstrument.Add(new(ProgressStatus.Busy)
            {
                Text = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_Text"),
                Progress = MaxCount <= 0 ? 1 : 0,
                Subtext = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_Subtext"),
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
                Subtext = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_SuccessText"),
                Progress = 1,
            });
        });
    }

    private void InstallProgressList()
    {
        var dashboard = Processor.GetService<DashboardService>();

        var listInstrument = dashboard.Create<ProgressListInstrument>(instrument =>
        {
            instrument.Title = Options.Title ?? LocalizationUtil.Localize("Loops_Instrument_Title");
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
                        Text = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_ProgressText", i + 1)
                    };
                    listInstrument.Add(new(info));
                }
            }
        });

        Hooks.Register<IterationStartingHook>(e =>
        {
            listInstrument.Update(e.Index, new(ProgressStatus.Busy)
            {
                Text = LocalizationUtil.Localize("Loops_Instrument_ProgressBar_ProgressText", e.Index + 1),
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
