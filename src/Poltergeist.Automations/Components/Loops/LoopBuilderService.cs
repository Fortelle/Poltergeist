using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Loops;

public class LoopBuilderService(MacroProcessor processor) : MacroService(processor)
{
    public string? Title { get; set; }

    public Action<LoopExecuteArguments>? Execute;
    public Action<LoopCheckContinueArguments>? CheckContinue;
    public Func<int, ProgressInstrumentInfo>? InitializeInfo;

    public bool ContinuesOnError { get; set; }
    public bool ExcludesIncompleteIteration { get; set; }
    public LoopInstrumentType InstrumentType { get; set; }
    public int MaxCount { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public int MaxIterationLimit { get; set; }
    public LoopStatus Status { get; private set; } = LoopStatus.None;
    public int IterationCount { get; private set; }
    public bool IsSticky { get; set; }

    public Exception? Exception { get; set; }

    private Action? LoopStarted;
    private Action? LoopEnded;
    private Action<int>? IterationStarted;
    private Action<int, IterationResult>? IterationEnded;
    private Action<int, ProgressInstrumentInfo>? IterationReported;

    public void Run()
    {
        InstallInstrument();

        DoLoop();
    }

    private void DoLoop()
    {
        Logger.Debug("Started running the loop procedure.");

        LoopStarted?.Invoke();

        var iterationIndex = 0;

        do
        {
            IterationStarted?.Invoke(iterationIndex);

            var iterationResult = DoIterate(iterationIndex);

            Processor.GetService<HookService>().Raise(new IterationEndedHook(iterationResult));

            IterationEnded?.Invoke(iterationIndex, iterationResult);

            IterationCount += iterationResult.Status switch
            {
                IterationStatus.Error when ExcludesIncompleteIteration => 0,
                IterationStatus.UserAborted when ExcludesIncompleteIteration => 0,
                _ => 1,
            };

            iterationIndex = DoCheckContinue(iterationResult);

        } while (iterationIndex > -1);

        LoopEnded?.Invoke();

        Logger.Debug("Finished running the loop procedure.");
    }

    private IterationResult DoIterate(int iterationIndex)
    {
        Logger.Debug($"Started running the iteration procedure.", new { iterationIndex });

        var startTime = DateTime.Now;
        var startElapsedTime = Processor.GetElapsedTime();

        Logger.Info($"Iteration: {iterationIndex + 1}");

        Processor.GetService<HookService>().Raise(new IterationStartedHook(iterationIndex, startTime));

        var status = IterationStatus.None;

        if (Execute is not null)
        {
            try
            {
                var args = Processor.GetService<LoopExecuteArguments>();
                args.Index = iterationIndex;
                args.StartTime = startTime;
                args.Result = IterationStatus.Continue;

                void onReported(ProgressInstrumentInfo info)
                {
                    info.ProgressMax ??= args.ProgressMax;
                    IterationReported?.Invoke(iterationIndex, info);
                }
                args.Reported += onReported;

                Execute.Invoke(args);
                status = args.Result;
                Processor.Comment = args.Comment;

                args.Reported -= onReported;
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
        var endElapsedTime = Processor.GetElapsedTime();

        Logger.Debug($"Finished running the iteration procedure.", new { iterationIndex, status });

        return new IterationResult()
        {
            Index = iterationIndex,
            StartTime = startTime,
            EndTime = endTime,
            Duration = endElapsedTime - startElapsedTime,
            Status = status,
        };
    }

    private int DoCheckContinue(IterationResult iterationResult)
    {
        Logger.Debug($"Started running the check-continue procedure.");

        var nextIterationIndex = -1;

        if (iterationResult.Status == IterationStatus.Stop)
        {
            Logger.Debug($"The loop will be stopped because the previous iteration has returned {iterationResult.Status}.");
        }
        else if (iterationResult.Status == IterationStatus.Error && !ContinuesOnError)
        {
            Status = LoopStatus.ErrorOccurred;
            Logger.Debug($"The loop will be stopped because an error has occurred in the previous iteration.");
        }
        else if (iterationResult.Status == IterationStatus.UserAborted || Processor.IsCancelled)
        {
            Status = LoopStatus.UserAborted;
            Logger.Debug($"The loop will be stopped due to user aborting.");
        }
        else if (CheckTimeout())
        {
        }
        else if (iterationResult.Status == IterationStatus.RestartLoop)
        {
            nextIterationIndex = 0;
            Logger.Debug($"The loop will be restarted because the previous iteration has returned {iterationResult.Status}.");
        }
        else if (iterationResult.Status == IterationStatus.RestartIteration)
        {
            nextIterationIndex = iterationResult.Index;
            Logger.Debug($"The iteration will be restarted because the previous iteration has returned {iterationResult.Status}.");
        }
        else if (CheckCount(iterationResult.Index))
        {
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the specified count.", new { iterationResult.Index, MaxCount });
        }
        else if (MaxIterationLimit > 0 && iterationResult.Index >= MaxIterationLimit - 1)
        {
            Logger.Debug($"The loop will be stopped because the number of iterations has reached the max iteration limit.", new { iterationResult.Index, MaxIterationLimit });
        }
        else if (CheckContinue is null)
        {
            nextIterationIndex = iterationResult.Index + 1;
            Logger.Debug($"The loop will be continued because the {nameof(CheckContinue)} is not defined.");
        }
        else if (iterationResult.Status == IterationStatus.ForceContinue)
        {
            nextIterationIndex = iterationResult.Index + 1;
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
            switch (state)
            {
                case CheckContinueResult.Break:
                    Logger.Debug($"The loop will be stopped because the {nameof(CheckContinue)} returns {state}.");
                    break;
                case CheckContinueResult.RestartLoop:
                    nextIterationIndex = 0;
                    Logger.Debug($"The loop will be restarted because the {nameof(CheckContinue)} returns {state}.");
                    break;
                case CheckContinueResult.RestartIteration:
                    nextIterationIndex = iterationResult.Index;
                    Logger.Debug($"The iteration will be restarted because the {nameof(CheckContinue)} returns {state}.");
                    break;
                default:
                    nextIterationIndex = iterationResult.Index + 1;
                    Logger.Debug($"The loop will be continued because the {nameof(CheckContinue)} returns {state}.");
                    break;
            }
        }

        Logger.Debug($"Finished running the check-continue procedure.");

        return nextIterationIndex;
    }

    private bool CheckCount(int iterationIndex)
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

    private bool CheckTimeout()
    {
        if (MaxDuration == default)
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

                    IterationStarted = (iterationIndex) =>
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
                                Progress = (iterationIndex + 1d) / (MaxCount + 1),
                                Subtext = $"{iterationIndex + 1} / {MaxCount}",
                            });
                        }
                    };

                    IterationReported = (iterationIndex, info) =>
                    {
                        var item = new ProgressListInstrumentItem(info);
                        item.Text ??= ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", iterationIndex + 1);
                        listInstrument.Update(1, item);
                    };

                    LoopEnded = () =>
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
                            var info = InitializeInfo?.Invoke(i) ?? new();
                            gridInstrument.Add(new(ProgressStatus.Idle)
                            {
                            });
                        }
                    }

                    IterationReported = (iterationIndex, info) =>
                    {
                        gridInstrument.Update(iterationIndex, new(info));
                    };

                    IterationEnded = (iterationIndex, result) =>
                    {
                        var status = result.Status switch
                        {
                            IterationStatus.Error => ProgressStatus.Failure,
                            _ => ProgressStatus.Success,
                        }; 
                        gridInstrument.Update(iterationIndex, new(status));
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
                            var info = InitializeInfo?.Invoke(i) ?? new();
                            info.Status ??= ProgressStatus.Idle;
                            info.Text ??= ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", i + 1);
                            listInstrument.Add(new(info));
                        }
                    }

                    IterationStarted = (iterationIndex) =>
                    {
                        listInstrument.Update(iterationIndex, new(ProgressStatus.Busy)
                        {
                            Text = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Instrument_ProgressBar_ProgressText", iterationIndex + 1),
                        });
                    };

                    IterationReported = (iterationIndex, info) =>
                    {
                        listInstrument.Update(iterationIndex, new(info));
                    };

                    IterationEnded = (iterationIndex, result) =>
                    {
                        var status = result.Status switch
                        {
                            IterationStatus.Error => ProgressStatus.Failure,
                            IterationStatus.UserAborted => ProgressStatus.Warning,
                            _ => ProgressStatus.Success,
                        };
                        listInstrument.Update(result.Index, new(status));
                    };

                }
                break;
        }
    }
}
