using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros.Loops;

public class LoopService : MacroService, IWorkflowExecutor
{
    public static readonly EntryDefinition<int> ReportIterationDefinition = new("loop_iterations");
    public static readonly EntryDefinition<bool> StopAfterCurrentDefinition = new("stop_after_current");
    
    private readonly HookService HookService;

    private LoopPattern Pattern;
    private int MaxIterations;
    private TimeSpan MaxDuration;

    private StartCallback? StartAsync;
    private ExecuteCallback? ExecuteAsync;
    private TransitionCallback? TransitionAsync;
    private EndCallback? EndAsync;
    private FinalizeCallback? Finalize;

    private int TotalIterations;

    public LoopService(
        MacroProcessor processor,
        HookService hookService
        ) : base(processor)
    {
        HookService = hookService;
    }

    public async Task<bool> Process()
    {
        Setup();

        if (!await InternalStart())
        {
            return false;
        }

        var iterationIndex = 0;

        while (true)
        {
            var isSuccess = await InternalIterate(iterationIndex);

            if (!isSuccess)
            {
                break;
            }

            if (!ShouldContinue())
            {
                break;
            }

            if (!await InternalTransition(iterationIndex))
            {
                break;
            }

            iterationIndex++;
        }

        return await InternalTEnd(iterationIndex);
    }

    private void Setup()
    {
        Logger.Trace("Started running the loop-setup procedure.");

        Processor.ThrowIfCancellationRequested();

        var hook = new LoopSetupHook();
        HookService.Raise(hook);

        Pattern = hook.Pattern;
        MaxIterations = Pattern switch
        {
            LoopPattern.Once => 1,
            LoopPattern.Unlimited => -1,
            _ => hook.MaxIterations,
        };
        MaxDuration = hook.MaxDuration;
        StartAsync = hook.StartAsync;
        ExecuteAsync = hook.ExecuteAsync;
        TransitionAsync = hook.TransitionAsync;
        EndAsync = hook.EndAsync;
        Finalize = hook.Finalize;

        Logger.Trace("Finished running the loop-setup procedure.");
    }

    private async Task<bool> InternalStart()
    {
        Logger.Trace("Started running the loop-start procedure.");


        Processor.ThrowIfCancellationRequested();

        var loopStartingHook = new LoopStartingHook()
        {
            Pattern = Pattern,
            MaxIterations = MaxIterations,
            MaxDuration = MaxDuration,
        };
        HookService.Raise(loopStartingHook);
        if (loopStartingHook.Cancel)
        {
            Logger.Trace($"The workflow will be stopped due to {nameof(LoopStartingHook)} cancellation.");
            return false;
        }


        Processor.ThrowIfCancellationRequested();

        if (StartAsync is not null)
        {
            var canStart = await StartAsync.Invoke(Processor);
            if (!canStart)
            {
                Logger.Trace($"The workflow will be stopped because the loop-start procedure has failed.");
                return false;
            }
        }


        Processor.ThrowIfCancellationRequested();

        var loopStartedHook = new LoopStartedHook()
        {
            Pattern = Pattern,
            MaxIterations = MaxIterations,
            MaxDuration = MaxDuration,
        };
        HookService.Raise(loopStartedHook);

        Logger.Trace("Finished running the loop-start procedure.");

        return true;
    }

    private async Task<bool> InternalIterate(int iterationIndex)
    {
        Logger.Trace($"Started running the iteration procedure.", new { iterationIndex });


        Processor.ThrowIfCancellationRequested();

        var iterationStartingHook = new IterationStartingHook()
        {
            Index = iterationIndex,
        };
        HookService.Raise(iterationStartingHook);
        if (iterationStartingHook.Cancel)
        {
            Logger.Trace($"The workflow will be stopped due to {nameof(IterationStartingHook)} cancellation.", new { iterationIndex });
            return false;
        }


        Processor.ThrowIfCancellationRequested();

        var iterationStartedHook = new IterationStartedHook()
        {
            Index = iterationIndex,
        };
        HookService.Raise(iterationStartedHook);


        Processor.ThrowIfCancellationRequested();

        var shouldContinue = true;
        if (ExecuteAsync is not null)
        {
            try
            {
                var context = new IterationContext()
                {
                    Index = iterationIndex,
                };

                await ExecuteAsync.Invoke(Processor, context);

                if (context.ShouldBreak == true)
                {
                    Logger.Trace($"The workflow will be stopped due to {nameof(IterationContext.ShouldBreak)} being set to true.");
                    shouldContinue = false;
                }
            }
            catch (IterationContinueException exception)
            {
                shouldContinue = true;
                Logger.Trace($"The iteration exited early with a continue conclusion.", new { exception.Message });
            }
            catch (IterationBreakException exception)
            {
                shouldContinue = false;
                Logger.Trace($"The iteration exited early with a break conclusion.", new { exception.Message });
            }
            catch (Exception exception) when (MacroProcessor.IsStopException(exception))
            {
                Logger.Trace($"The workflow will be stopped due to user cancellation.");

                HookService.Raise(new IterationCanceledHook()
                {
                    Index = iterationIndex,
                });
                throw;
            }
            catch (Exception exception)
            {
                Logger.Trace($"The workflow will be stopped because an error has occurred during the iteration.");

                HookService.Raise(new IterationErrorHook()
                {
                    Index = iterationIndex,
                    Exception = exception,
                });
                throw;
            }
        }

        TotalIterations++;


        Processor.ThrowIfCancellationRequested();

        var iterationEndedHook = new IterationEndedHook()
        {
            Index = iterationIndex,
        };
        HookService.Raise(iterationEndedHook);


        Logger.Trace($"Finished running the iteration procedure.");

        return shouldContinue;
    }

    private bool ShouldContinue()
    {
        Processor.ThrowIfCancellationRequested();

        if (IsMax())
        {
            Logger.Trace($"The workflow will be stopped because the number of iterations has reached the specified count.", new { TotalIterations, MaxIterations });
            return false;
        }
        else if (IsTimeout(out var duration))
        {
            Logger.Trace($"The workflow will be stopped because the run duration has reached the specified time.", new { duration, MaxDuration });
            return false;
        }
        else if (CheckStopAfterCurrent())
        {
            Logger.Trace($"The workflow will be stopped because the user requested to stop after the current iteration.");
            return false;
        }

        return true;

        bool IsMax()
        {
            return Pattern switch
            {
                LoopPattern.Once => true,
                LoopPattern.Multiple when MaxIterations <= 1 => true,
                LoopPattern.Multiple when TotalIterations >= MaxIterations => true,
                LoopPattern.Multiple => false,
                LoopPattern.Unlimited => false,
                _ => throw new NotSupportedException(),
            };
        }

        bool IsTimeout(out TimeSpan duration)
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

        bool CheckStopAfterCurrent()
        {
            if (Processor.SessionStorage.TryGetValue(StopAfterCurrentDefinition, out var stopAfterCurrent))
            {
                return stopAfterCurrent;
            }

            return false;
        }
    }

    private async Task<bool> InternalTransition(int iterationIndex)
    {
        Logger.Trace($"Started running the transition procedure.");


        Processor.ThrowIfCancellationRequested();

        var shouldContinue = true;
        if (TransitionAsync is not null)
        {
            var context = new TransitionContext()
            {
                Index = iterationIndex,
            };

            await TransitionAsync.Invoke(Processor, context);

            if (context.Cancel)
            {
                shouldContinue = false;
                Logger.Trace($"The workflow will be stopped due to {nameof(TransitionContext.Cancel)} being set to true.");
            }
        }


        Logger.Trace($"Finished running the transition procedure.");

        return shouldContinue;
    }

    private async Task<bool> InternalTEnd(int totalIterations)
    {
        Logger.Trace("Started running the loop-end procedure.");


        Processor.ThrowIfCancellationRequested();

        HookService.Raise(new LoopEndingHook()
        {
            Iterations = totalIterations,
        });


        Processor.ThrowIfCancellationRequested();

        if (EndAsync is not null)
        {
            await EndAsync.Invoke(Processor);
        }


        Processor.ThrowIfCancellationRequested();

        HookService.Raise(new LoopEndedHook()
        {
            Iterations = totalIterations,
        });


        Logger.Trace("Finished running the loop-end procedure.");

        return true;
    }

    public void Cleanup()
    {
        Logger.Trace("Started running the cleanup procedure.");


        if (Finalize is not null)
        {
            Finalize.Invoke(Processor);
        }


        Processor.Report.Add(ProcessorResult.CommentDefinition, LocalizationUtil.Localize("Loops_Comment", TotalIterations));
        Processor.Report.Add(ReportIterationDefinition, TotalIterations);


        Logger.Trace("Finished running the cleanup procedure.");
    }
}
