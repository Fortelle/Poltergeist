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

    private int IterationIndex = -1;

    public Func<bool>? BeforeProc { get; set; }
    public Func<int, bool>? IterationProc { get; set; }
    public Func<int, bool>? IntermissionProc { get; set; }
    public Action? AfterProc { get; set; }

    public LoopService(MacroProcessor processor,
        HookService hooks
        ) : base(processor)
    {
        Hooks = hooks;

        processor.AddStep(new("loop-initialization", DoInitialization)
        {
            SuccessStepId = "loop-before",
            IsDefault = true,
        });

        processor.AddStep(new("loop-before", DoBefore)
        {
            IsInterruptable = true,
            SuccessStepId = "loop-iteration",
            FailureStepId = "loop-after",
            ErrorStepId = "loop-error",
        });

        processor.AddStep(new("loop-iteration", DoIteration)
        {
            IsInterruptable = true,
            Finally = DoIterationFinally,
            SuccessStepId = "loop-intermission",
            FailureStepId = "loop-after",
            ErrorStepId = "loop-error",
        });

        processor.AddStep(new("loop-intermission", DoIntermission)
        {
            IsInterruptable = true,
            SuccessStepId = "loop-iteration",
            FailureStepId = "loop-after",
            ErrorStepId = "loop-error",
        });

        processor.AddStep(new("loop-after", DoAfter)
        {
            IsInterruptable = true,
            SuccessStepId = "loop-finalization",
            ErrorStepId = "loop-error",
        });

        processor.AddStep(new("loop-error", DoError)
        {
            SuccessStepId = "loop-finalization",
        });

        processor.AddStep(new("loop-finalization", DoFinalization)
        {
        });

        Hooks.Register<ProcessorEndingHook>(OnProcessorEnding);
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


    private bool DoBefore(WorkflowStepArguments stepArguments)
    {
        var startHook = new LoopStartingHook();
        Hooks.Raise(startHook);

        var canStart = BeforeProc?.Invoke() != false;

        if (!canStart)
        {
            Result = LoopResult.Unstarted;
        }

        var startedHook = new LoopStartedHook()
        {
            IsCancelled = !canStart,
        };
        Hooks.Raise(startedHook);

        return canStart;
    }

    private bool DoIteration(WorkflowStepArguments stepArguments)
    {
        IterationIndex++;

        Logger.Info($"Loop: {IterationIndex + 1}");

        var startHook = new IterationStartingHook()
        {
            Index = IterationIndex,
            StartTime = stepArguments.StartTime,
        };
        Hooks.Raise(startHook);

        var isSuccess = IterationProc?.Invoke(IterationIndex) != false;

        return isSuccess;
    }

    private void DoIterationFinally(WorkflowStepFinallyArguments stepArguments)
    {
        var canContinue = CheckContinue(stepArguments.State);

        stepArguments.NextStepId = canContinue ? "loop-intermission" : "loop-after";

        var endHook = new IterationEndHook()
        {
            Index = IterationIndex,
            StartTime = stepArguments.StartTime,
            EndTime = stepArguments.EndTime,
            Duration = stepArguments.Duration,
            State = stepArguments.State,
        };
        Hooks.Raise(endHook);
    }

    private bool DoIntermission(WorkflowStepArguments stepArguments)
    {
        return IntermissionProc?.Invoke(IterationIndex) != false;
    }

    private bool CheckContinue(WorkflowStepState state)
    {
        if (state == WorkflowStepState.Failed)
        {
            Result = LoopResult.Broken;
            Logger.Debug($"The loop will be stopped because the previous iteration has failed.");
            return false;
        }
        else if (state == WorkflowStepState.Error)
        {
            Result = LoopResult.Error;
            Logger.Debug($"The loop will be stopped because an error has occurred in the previous iteration.");
            return false;
        }
        else if (state == WorkflowStepState.Interrupted)
        {
            Result = LoopResult.Interrupted;
            Logger.Debug($"The loop will be stopped due to user interruption.");
            return false;
        }
        else if (Processor.Status == ProcessorStatus.Stopping)
        {
            Result = LoopResult.Interrupted;
            Logger.Debug($"The loop will be stopped because the processor is stopping.");
            return false;
        }
        else if (CheckTimeout(out var duration))
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the run duration has reached the specified time.", new { duration, MaxDuration });
            return false;
        }
        else if (CheckCount())
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the specified count.", new { IterationIndex, MaxCount });
            return false;
        }
        else if (Options.MaxIterationLimit > 0 && IterationIndex >= Options.MaxIterationLimit - 1)
        {
            Result = LoopResult.Complete;
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the max iteration limit.", new { IterationIndex, Options.MaxIterationLimit });
            return false;
        }

        return true;

        bool CheckCount()
        {
            if (MaxCount == 0)
            {
                return false;
            }

            if (IterationIndex >= MaxCount - 1)
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
    }

    private void DoAfter(WorkflowStepArguments stepArguments)
    {
        Processor.Report.TryAdd("comment_message", LocalizationUtil.Localize("Loops_Comment", IterationIndex + 1));

        var iterations = IterationIndex + 1;

        var endingHook = new LoopEndingHook()
        {
            Result = Result,
            Iterations = iterations,
        };
        Hooks.Raise(endingHook);
    }

    private void DoError(WorkflowStepArguments stepArguments)
    {
        Result = LoopResult.Error;
    }

    private void DoFinalization()
    {
        Hooks.Raise<LoopFinalizedHook>();
    }

    private void OnProcessorEnding(ProcessorEndingHook hook)
    {
        var iterations = IterationIndex + 1;

        var endHook = new LoopEndedHook()
        {
            Result = Result,
            Iterations = iterations,
        };
        Hooks.Raise(endHook);

        hook.Report.Add(ReportIterationCountKey, iterations);
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
            var status = e.State switch
            {
                WorkflowStepState.Failed => ProgressStatus.Failure,
                WorkflowStepState.Error => ProgressStatus.Failure,
                WorkflowStepState.Interrupted => ProgressStatus.Warning,
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
            var status = e.State switch
            {
                WorkflowStepState.Failed => ProgressStatus.Failure,
                WorkflowStepState.Error => ProgressStatus.Failure,
                WorkflowStepState.Interrupted => ProgressStatus.Warning,
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
