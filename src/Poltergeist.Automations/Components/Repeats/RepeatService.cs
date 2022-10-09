using System;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Components.Loops;

public class RepeatService : MacroService
{
    public const string ConfigEnableKey = "repeat-enable";
    public const string ConfigCountKey = "repeat-count";
    public const string ConfigTimeoutKey = "repeat-timeout";
    public const string InstrumetKey = "repeat-instrumet";

    private readonly HookService Hooks;

    public int IterationIndex;
    private int LoopCount;
    private TimeSpan LoopTimeout;
    public bool SoftStop;

    public Action<LoopBeginArguments> BeginProc;
    public Action<LoopIterationArguments> IterationProc;
    public Action<LoopCheckNextArguments> CheckNextProc;
    public Action<ArgumentService> EndProc;

    private readonly RepeatOptions Options;

    public RepeatService(MacroProcessor processor,
        WorkingService work,
        HookService hooks,
        IOptions<RepeatOptions> options
        ) : base(processor)
    {
        Hooks = hooks;
        Options = options.Value;

        CheckLimit();

        work.WorkProc = WorkProc;
    }

    private void CheckLimit()
    {
        var useLoop = Processor.GetOption<bool>(ConfigEnableKey);

        if (!useLoop) return;

        if (Options?.UseCount == true)
        {
            LoopCount = Processor.GetOption<int>(ConfigCountKey);
        }
        if (Options?.UseTimeout == true)
        {
            var seconds = Processor.GetOption<int>(ConfigTimeoutKey);
            LoopTimeout = TimeSpan.FromSeconds(seconds);
        }

        if (LoopCount == 0 && LoopTimeout == TimeSpan.Zero)
        {
            if (Options?.AllowInfiniteLoop == true)
            {
                LoopCount = 0;
            }
            else
            {
                LoopCount = 1;
                Logger.Warn($"Neither {ConfigCountKey} nor {ConfigTimeoutKey} is set. To enable infinite loop, set ${nameof(Options.AllowInfiniteLoop)} to true.");
            }
        }
    }

    private EndReason Status = EndReason.None;

    private EndReason WorkProc()
    {
        CreateInstrument();

        if (!DoBegin())
        {
            return EndReason.Unstarted;
        }

        DoLoop();

        DoEnd();

        if(Status == EndReason.None)
        {
            Status = EndReason.Complete;
        }

        return Status;
    }

    private bool DoBegin()
    {
        Logger.Debug("Started running the before-loop procedure.");

        var canBegin = true;

        if (BeginProc is not null)
        {
            var args = Processor.GetService<LoopBeginArguments>();
            BeginProc.Invoke(args);
            if (args.Cancel)
            {
                canBegin = false;
            }
        }

        Hooks.Raise("repeat_begin");

        Logger.Debug("Finished running the before-loop procedure.", new {canBegin});

        return canBegin;
    }

    private void DoLoop()
    {
        Logger.Debug("Started running the loop-body procedure.");

        IterationIndex = -1;

        while (true)
        {
            IterationIndex++;

            var iterationState = DoIterate(IterationIndex);

            var restart = DoCheck(iterationState);

            if (!restart) break;

        }

        Logger.Debug("Finished running the loop-body procedure.");
    }

    private IterationResult DoIterate(int index)
    {
        Logger.Debug($"Started running the loop iteration.", new { index });

        var beginTime = DateTime.Now;

        Logger.Info($"Repeat: {IterationIndex + 1}");

        UpdateInstrumentBefore(index);

        Hooks.Raise("iteration_begin", index, beginTime);

        var state = IterationResult.Null;

        if (IterationProc is not null)
        {
            try
            {
                var args = Processor.GetService<LoopIterationArguments>();
                args.Index = index;
                args.BeginTime = beginTime;
                args.Result = IterationResult.Continue;
                IterationProc.Invoke(args);
                state = args.Result;
            }
            catch (Exception)
            {
                state = IterationResult.Error;
            }
        }

        var endTime = DateTime.Now;
        Hooks.Raise("iteration_end", index, beginTime, endTime, state);

        UpdateInstrumentAfter(index, beginTime, endTime);

        Logger.Debug($"Finished running the loop iteration.", new {index, state});
        
        return state;
    }

    private bool DoCheck(IterationResult iterationResult)
    {
        Logger.Debug($"Started running the checknext procedure.");

        var hasNext = false;

        if (iterationResult == IterationResult.Stop)
        {
            hasNext = false;
        }
        if (iterationResult == IterationResult.Error && Options?.StopOnError == true)
        {
            hasNext = false;
            Status = EndReason.ErrorOccurred;
            Logger.Debug($"StopOnError");
        }
        else if (SoftStop)
        {
            hasNext = false;
            Status = EndReason.UserAborted;
            Logger.Debug("SoftStop");
        }
        else if (CheckCount())
        {
            hasNext = false;
        }
        else if (CheckTimeout())
        {
            hasNext = false;
        }
        else if (iterationResult == IterationResult.ForceRestart)
        {
            hasNext = true;
        }
        else if (CheckNextProc is null)
        {
            hasNext = true;
            Logger.Debug($"{nameof(CheckNextProc)} is null.");
        }
        else
        {
            var args = Processor.GetService<LoopCheckNextArguments>();
            CheckNextProc.Invoke(args);
            var state = args.Result;
            hasNext = state switch
            {
                CheckNextResult.Continue => true,
                CheckNextResult.Break => false,
                _ => true,
            };
            Logger.Debug($"{nameof(CheckNextProc)} returned {state}.");
        }

        Hooks.Raise("repeat_check", hasNext);

        Logger.Debug($"Finished running the checknext procedure.", new { hasNext });

        return hasNext;
    }

    private bool CheckCount()
    {
        if (LoopCount == 0) return false;

        if (IterationIndex >= LoopCount - 1)
        {
            Logger.Debug($"Max iteration reached.", new { IterationIndex, LoopCount });
            return true;
        }

        return false;
    }

    private bool CheckTimeout()
    {
        if (LoopTimeout == TimeSpan.Zero) return false;
        var now = DateTime.Now;
        var startTime = Processor.StartTime;
        var duration = now - startTime;
        if (duration > LoopTimeout)
        {
            Logger.Debug($"Timeout reached.", new { now, startTime, duration, LoopTimeout });
            return true;
        }

        return false;
    }

    private void DoEnd()
    {
        Logger.Debug($"Started running the after-loop procedure.");

        if (EndProc is not null)
        {
            var args = Processor.GetService<ArgumentService>();
            EndProc.Invoke(args);
        }

        Processor.SetStatistic<int>("TotalRepeatCount", old => old + IterationIndex + 1);

        Hooks.Raise("repeat_end");

        Logger.Debug($"Finished running the after-loop procedure.");
    }






    // todo: move to a new service
    private void CreateInstrument()
    {
        switch (Options?.Instrument)
        {
            case RepeatInstrumentType.ProgressBar:
                {
                }
                break;
            case RepeatInstrumentType.List:
                {
                    var instruments = Processor.GetService<InstrumentService>();
                    instruments.Create<ListInstrument>(li =>
                    {
                        li.Key = InstrumetKey;
                        li.Title = "Repeats:";

                        if (LoopCount > 0)
                        {
                            for (var i = 0; i < LoopCount; i++)
                            {
                                li.Add(new()
                                {
                                    Key = $"loop_{i}",
                                    Status = ProgressStatus.Idle,
                                    Text = $"Loop {i + 1}."
                                });
                            }
                        }
                    });
                }
                break;
            case RepeatInstrumentType.Grid:
                {
                    var instruments = Processor.GetService<InstrumentService>();
                    instruments.Create<GridInstrument>(gi =>
                    {
                        gi.Key = InstrumetKey;
                        gi.Title = "Repeats:";

                        if (LoopCount > 0)
                        {
                            gi.SetPlaceholders(LoopCount);
                        }
                    });
                }
                break;
        }
    }

    private void UpdateInstrumentBefore(int index)
    {
        switch (Options?.Instrument)
        {
            case RepeatInstrumentType.List:
                {
                    var instruments = Processor.GetService<InstrumentService>();
                    instruments.Update<ListInstrument>(InstrumetKey, li =>
                    {
                        li.Update(index, new()
                        {
                            Status = ProgressStatus.Busy,
                            Text = $"Loop {index + 1}",
                        });
                    });
                }
                break;
            case RepeatInstrumentType.Grid:
                {
                    var instruments = Processor.GetService<InstrumentService>();
                    instruments.Update<GridInstrument>(InstrumetKey, gi =>
                    {
                        gi.Update(index, new(ProgressStatus.Busy));
                    });
                }
                break;
        }
    }

    private void UpdateInstrumentAfter(int index, DateTime begin, DateTime end)
    {
        var dur = end - begin;

        switch (Options?.Instrument)
        {
            case RepeatInstrumentType.List:
                {
                    var instruments = Processor.GetService<InstrumentService>();
                    instruments.Update<ListInstrument>(InstrumetKey, li =>
                    {
                        li.Update(index, new()
                        {
                            Status = ProgressStatus.Succeeded,
                            Text = $"Loop {index + 1}: Done",
                            Subtext = dur.ToString()
                        });
                    });
                }
                break;
            case RepeatInstrumentType.Grid:
                {
                    var instruments = Processor.GetService<InstrumentService>();
                    instruments.Update<GridInstrument>(InstrumetKey, gi =>
                    {
                        gi.Update(index, new(ProgressStatus.Succeeded));
                    });
                }
                break;
        }
    }
}
