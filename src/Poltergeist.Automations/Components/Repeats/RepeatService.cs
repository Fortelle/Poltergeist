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
    public const string ConfigTimesKey = "repeat-count";
    public const string ConfigPersistenceKey = "repeat-persistence";
    public const string InstrumetKey = "repeat-instrumet";

    private readonly HookService Hooks;

    public int IterationIndex;
    public int MaxIteration;
    public bool SoftStop;

    public Func<bool> BeginProc;
    public Func<IterationResult> IterationProc;
    public Func<CheckNextResult> CheckNextProc;
    public Action EndProc;

    private readonly RepeatOptions Options;

    public RepeatService(MacroProcessor processor,
        WorkingService work,
        HookService hooks,
        IOptions<RepeatOptions> options
        ) : base(processor)
    {
        Hooks = hooks;
        Options = options.Value;

        var useLoop = Processor.GetOption<bool>(ConfigEnableKey);
        var loopLimit = Processor.GetOption<int>(ConfigTimesKey);

        MaxIteration = 1;
        if (useLoop)
        {
            if (loopLimit == 0)
            {
                if (Options?.AllowInfinityLoop ?? false)
                {
                    MaxIteration = -1;
                }
            }
            else
            {
                MaxIteration = loopLimit;
            }
        }

        work.WorkProc = WorkProc;
    }

    private void WorkProc()
    {
        CreateInstrument();

        if (!DoBegin())
        {
            return;
        }

        DoLoop();

        DoEnd();
    }

    private bool DoBegin()
    {
        Log(LogLevel.Debug, "Started running the before-loop process.");

        if (BeginProc is not null)
        {
            BeginProc.Invoke();
        }

        Hooks.Raise("repeat_begin");

        Log(LogLevel.Debug, "Finished running the before-loop process.");

        return true;
    }

    private void DoLoop()
    {
        Log(LogLevel.Debug, "Started running the loop-body process.");

        IterationIndex = -1;

        while (true)
        {
            IterationIndex++;

            var iterationState = DoIterate(IterationIndex);

            var restart = DoCheck(iterationState);

            if (!restart) break;

        }

        Log(LogLevel.Debug, "Finished running the loop-body process.");
    }

    private IterationResult DoIterate(int index)
    {
        Log(LogLevel.Debug, $"Started running the loop-iteration {index}.");

        var beginTime = DateTime.Now;

        UpdateInstrumentBefore(index);

        Hooks.Raise("iteration_begin", index, beginTime);

        var state = IterationResult.Null;

        if (IterationProc is not null)
        {
            try
            {
                state = IterationProc.Invoke();
            }
            catch (Exception e)
            {
                state = IterationResult.Error;
            }
        }

        var endTime = DateTime.Now;
        Hooks.Raise("iteration_end", index, beginTime, endTime, state);

        UpdateInstrumentAfter(index, beginTime, endTime);

        Log(LogLevel.Information, $"Repeat: {IterationIndex + 1}");

        Log(LogLevel.Debug, $"Finished running the loop-iteration {index}: {state}.");
        
        return state;
    }

    private bool DoCheck(IterationResult iterationResult)
    {
        Log(LogLevel.Debug, $"Started running the checknext process.");

        var hasNext = false;
        var duration = Options?.UsePersistence == true ? Processor.GetOption<TimeSpan>(ConfigPersistenceKey) : default;

        if (iterationResult == IterationResult.Stop)
        {
            hasNext = false;
            Log (LogLevel.Debug, $"Iteration returned {iterationResult}.");
        }
        if (iterationResult == IterationResult.Error && Options?.StopOnError == true)
        {
            hasNext = false;
            Log(LogLevel.Debug, $"StopOnError");
        }
        else if (SoftStop)
        {
            hasNext = false;
            Log(LogLevel.Debug, "SoftStop");
        }
        else if (MaxIteration > 0 && IterationIndex >= MaxIteration - 1)
        {
            hasNext = false;
            Log(LogLevel.Debug, $"Max iteration reached: {MaxIteration}.");
        }
        else if (CheckMaxPersistence())
        {
            hasNext = false;
        }
        else if (iterationResult == IterationResult.ForceRestart)
        {
            hasNext = true;
            Log(LogLevel.Debug, $"Iteration returned {iterationResult}.");
        }
        else if (CheckNextProc is null)
        {
            hasNext = true;
            Log(LogLevel.Debug, $"{nameof(CheckNextProc)} is null.");
        }
        else
        {
            var state = CheckNextProc.Invoke();
            hasNext = state switch
            {
                CheckNextResult.Continue => true,
                CheckNextResult.Break => false,
                _ => true,
            };
            Log(LogLevel.Debug, $"{nameof(CheckNextProc)} returned {state}.");
        }

        Hooks.Raise("repeat_check", hasNext);

        Log(LogLevel.Debug, $"Finished running the checknext process: {hasNext}.");

        return hasNext;
    }

    private bool CheckMaxPersistence()
    {
        if (Options?.UsePersistence == false) return false;
        var duration = Processor.GetOption<TimeSpan>(ConfigPersistenceKey);
        if (duration == TimeSpan.Zero) {
            return false;
        }
        else if (DateTime.Now - Processor.StartTime > duration) 
        {
            Log(LogLevel.Debug, $"Max persistence reached: {duration}.");
            return true;
        }
        else
        {
            return false;
        }
    }


    private void DoEnd()
    {
        Log(LogLevel.Debug, $"Started running the after-loop process.");

        if (EndProc is not null)
        {
            EndProc.Invoke();
        }

        Processor.Macro.Environments.Set<int>("TotalRepeatCount", old => IterationIndex + 1);

        Hooks.Raise("repeat_end");

        Log(LogLevel.Debug, $"Finished running the after-loop process.");

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

                        if (MaxIteration > 0)
                        {
                            for (var i = 0; i < MaxIteration; i++)
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

                        if (MaxIteration > 0)
                        {
                            gi.SetPlaceholders(MaxIteration);
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
