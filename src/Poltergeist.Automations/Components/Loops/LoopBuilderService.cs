using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Components.Repetitions;
using Poltergeist.Automations.Exceptions;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Loops;

public class LoopBuilderService : MacroService
{
    public string? Title { get; set; }

    public Action<LoopExecutionArguments>? Execution;
    public Action<LoopCheckContinueArguments>? CheckContinue;

    public bool IsSticky { get; set; }

    public LoopInstrumentType InstrumentType { get; set; }
    public bool ContinuesOnError { get; set; }
    public int MaxCount { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public int MaxIterationLimit { get; set; }
    public LoopStatus Status { get; set; } = LoopStatus.None;

    public Exception? Exception { get; set; }
    public int IteratedCount => IterationIndex;

    private int IterationIndex;
    private event Action? LoopStarted;
    private event Action? LoopEnded;
    private event Action? IterationStarted;
    private event Action<IterationResult>? IterationEnded;
    private event Action<int, int>? IterationProgressChanged;

    public LoopBuilderService(MacroProcessor processor) : base(processor)
    {
    }

    public void Execute()
    {
        InstallInstrument();

        DoLoop();
    }

    private void DoLoop()
    {
        Logger.Debug("Started running the loop procedure.");

        LoopStarted?.Invoke();

        while (true)
        {
            IterationStarted?.Invoke();

            var iterationResult = DoIterate();

            IterationEnded?.Invoke(iterationResult);

            var shouldContinue = DoCheckContinue(iterationResult);

            if (!shouldContinue)
            {
                break;
            }

            IterationIndex++;
        }

        LoopEnded?.Invoke();

        Logger.Debug("Finished running the loop procedure.");
    }

    private IterationResult DoIterate()
    {
        Logger.Debug($"Started running the iteration procedure.", new { IterationIndex });

        var startTime = DateTime.Now;

        Logger.Info($"Iteration: {IterationIndex + 1}");

        var status = IterationStatus.None;

        if (Execution is not null)
        {
            try
            {
                var args = Processor.GetService<LoopExecutionArguments>();
                args.Index = IterationIndex;
                args.StartTime = startTime;
                args.Result = IterationStatus.Continue;
                var max = 0;
                var current = 0;

                args.PropertyChanged += (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(LoopExecutionArguments.ProgressMax):
                            max = args.ProgressMax;
                            break;
                        case nameof(LoopExecutionArguments.ProgressValue):
                            current = args.ProgressValue;
                            break;
                    }
                    IterationProgressChanged?.Invoke(current, max);
                };

                Execution.Invoke(args);
                status = args.Result;
                Processor.Comment = args.Comment;
            }
            catch (UserAbortException e)
            {
                status = IterationStatus.UserAborted;
                Exception = e;
            }
            catch (ThreadInterruptedException e)
            {
                status = IterationStatus.UserAborted;
                Exception = e;
            }
            catch (Exception e)
            {
                status = IterationStatus.Error;
                Exception = e;
            }
        }

        var endTime = DateTime.Now;

        Logger.Debug($"Finished running the iteration procedure.", new { IterationIndex, status });

        return new IterationResult()
        {
            Index = IterationIndex,
            StartTime = startTime,
            EndTime = endTime,
            Status = status,
        };
    }

    private bool DoCheckContinue(IterationResult iterationResult)
    {
        Logger.Debug($"Started running the check-continue procedure.");

        var shouldContinue = false;

        if (iterationResult.Status == IterationStatus.Stop)
        {
            shouldContinue = false;
            Logger.Debug($"The loop will be stopped because the previous iteration has returned {iterationResult.Status}.");
        }
        else if (iterationResult.Status == IterationStatus.Error && !ContinuesOnError)
        {
            shouldContinue = false;
            Status = LoopStatus.ErrorOccurred;
            Logger.Debug($"The loop will be stopped because an error has occurred.");
        }
        else if (iterationResult.Status == IterationStatus.UserAborted || Processor.IsCancelled)
        {
            shouldContinue = false;
            Status = LoopStatus.UserAborted;
            Logger.Debug($"The loop will be stopped due to user abort.");
        }
        else if (CheckTimeout())
        {
            shouldContinue = false;
        }
        else if (iterationResult.Status == IterationStatus.RestartLoop)
        {
            shouldContinue = true;
            IterationIndex = -1;
            Logger.Debug($"The loop will be restarted because the previous iteration has returned {iterationResult.Status}.");
        }
        else if (iterationResult.Status == IterationStatus.RestartIteration)
        {
            shouldContinue = true;
            IterationIndex--;
            Logger.Debug($"The iteration will be restarted because the previous iteration has returned {iterationResult.Status}.");
        }
        else if (CheckCount())
        {
            shouldContinue = false;
        }
        else if (MaxIterationLimit > 0 && IterationIndex >= MaxIterationLimit - 1)
        {
            shouldContinue = false;
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the max iteration limit.", new { IterationIndex, MaxIterationLimit });
        }
        else if (CheckContinue is null)
        {
            shouldContinue = true;
            Logger.Debug($"The loop will be continued because the {nameof(CheckContinue)} is not defined.");
        }
        else if (iterationResult.Status == IterationStatus.ForceContinue)
        {
            shouldContinue = true;
            Logger.Debug($"The loop will be continued because the previous iteration has returned {iterationResult.Status}.");
        }
        else
        {
            var args = Processor.GetService<LoopCheckContinueArguments>();
            args.IterationIndex = iterationResult.Index;
            args.IterationResult = iterationResult;
            args.Result = CheckContinueResult.NotSet;
            CheckContinue.Invoke(args);
            var state = args.Result;
            Processor.Comment = args.Comment;
            shouldContinue = state switch
            {
                CheckContinueResult.Continue => true,
                CheckContinueResult.Break => false,
                _ => true,
            };
            if (shouldContinue)
            {
                Logger.Debug($"The loop will be continued because the {nameof(CheckContinue)} returns {state}.");
            }
            else
            {
                Logger.Debug($"The loop will be stopped because the {nameof(CheckContinue)} returns {state}.");
            }
        }

        Logger.Debug($"Finished running the check-continue procedure.", new { shouldContinue });

        return shouldContinue;
    }

    private bool CheckCount()
    {
        if (MaxCount == 0)
        {
            return false;
        }

        if (IterationIndex >= MaxCount - 1)
        {
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the specified count.", new { IterationIndex, MaxCount });
            return true;
        }

        return false;
    }

    private bool CheckTimeout()
    {
        if (MaxDuration == TimeSpan.Zero)
        {
            return false;
        }

        var now = DateTime.Now;
        var startTime = Processor.StartTime;
        var duration = now - startTime;
        if (duration > MaxDuration)
        {
            Logger.Debug($"The loop will be stopped because the run duration has reached the specified time.", new { now, startTime, duration, MaxDuration });
            return true;
        }

        return false;
    }

    private void InstallInstrument()
    {
        // todo: subtext: starttime, endtime, duration...

        var dashboard = Processor.GetService<DashboardService>();

        switch (InstrumentType)
        {
            case LoopInstrumentType.ProgressBar:
                {
                    var listInstrument = dashboard.Create<ProgressListInstrument>(instrument =>
                    {
                        instrument.Title = Title ?? ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_Title");
                        instrument.IsSticky = IsSticky;
                    });

                    listInstrument.Add(new(ProgressStatus.Busy)
                    {
                        Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_Text"),
                        Progress = MaxCount <= 0 ? 1 : 0,
                        Subtext = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_Subtext"),
                    });

                    IterationStarted += () =>
                    {
                        if (MaxCount <= 0)
                        {
                            listInstrument.Update(0, new()
                            {
                            });
                        }
                        else
                        {
                            listInstrument.Update(0, new()
                            {
                                Progress = (IterationIndex + 1d) / (MaxCount + 1),
                                Subtext = $"{IterationIndex + 1} / {MaxCount}",
                            });
                        }
                    };

                    IterationProgressChanged += (current, max) =>
                    {
                        listInstrument.Update(1, new()
                        {
                            Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", IterationIndex + 1),
                            Progress = GetProgress(current, max),
                        });
                    };

                    LoopEnded += () =>
                    {
                        if (MaxCount <= 0)
                        {
                            listInstrument.Update(0, new(ProgressStatus.Success)
                            {
                                Subtext = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_SuccessText"),
                            });
                        }
                        else
                        {
                            listInstrument.Update(0, new(ProgressStatus.Success)
                            {
                                Progress = 1,
                            });
                        }
                    };

                }
                break;
            case LoopInstrumentType.Grid:
                {
                    var gridInstrument = dashboard.Create<ProgressGridInstrument>(instrument =>
                    {
                        instrument.Title = Title ?? ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_Title");
                        instrument.IsSticky = IsSticky;
                    });

                    if (MaxCount > 0)
                    {
                        for (var i = 0; i < MaxCount; i++)
                        {
                            gridInstrument.Add(new(ProgressStatus.Idle)
                            {
                            });
                        }
                    }

                    IterationStarted += () =>
                    {
                        gridInstrument.Update(IterationIndex, new(ProgressStatus.Busy)
                        {
                        });
                    };

                    IterationEnded += (result) =>
                    {
                        var status = result.Status switch
                        {
                            IterationStatus.Error => ProgressStatus.Failure,
                            _ => ProgressStatus.Success,
                        }; 
                        gridInstrument.Update(IterationIndex, new(status));
                    };
                }
                break;
            case LoopInstrumentType.List:
                {
                    var listInstrument = dashboard.Create<ProgressListInstrument>(instrument =>
                    {
                        instrument.Title = Title ?? ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_Title");
                        instrument.IsSticky = IsSticky;
                    });

                    if (MaxCount > 0)
                    {
                        for (var i = 0; i < MaxCount; i++)
                        {
                            listInstrument.Add(new(ProgressStatus.Idle)
                            {
                                Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", i + 1),
                            });
                        }
                    }

                    IterationStarted += () =>
                    {
                        listInstrument.Update(IterationIndex, new(ProgressStatus.Busy)
                        {
                            Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", IterationIndex + 1),
                        });
                    };

                    IterationProgressChanged += (current, max) =>
                    {
                        listInstrument.Update(IterationIndex, new()
                        {
                            Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", IterationIndex + 1),
                            Progress = GetProgress(current, max),
                        });
                    };

                    IterationEnded += (result) =>
                    {
                        var status = result.Status switch
                        {
                            IterationStatus.Error => ProgressStatus.Failure,
                            IterationStatus.UserAborted => ProgressStatus.Warning,
                            _ => ProgressStatus.Success,
                        };
                        listInstrument.Update(IterationIndex, new(status));
                    };

                }
                break;
        }

    }

    private static double GetProgress(int current, int max)
    {
        if (max > 0)
        {
            return (double)current / max;
        }
        else
        {
            return 1;
        }
    }
}
