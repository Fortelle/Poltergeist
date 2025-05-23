﻿using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.Debugger;
using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    private readonly List<WorkflowStep> Steps = new();

    private bool CanAbort { get; set; }

    private List<string> Footsteps { get; } = new();

    private Thread? ProcessThread;

    private Thread? WorkflowThread;

    private void InternalRun()
    {
        ProcessThread = new Thread(() => {
            try
            {
                Launch();

                Process();
            }
            catch (Exception exception)
            {
                Exception = exception;
                Status = ProcessorStatus.Crushed;
            }
            finally
            {
                Finally();
            }
        });
        ProcessThread.SetApartmentState(ApartmentState.STA);
        ProcessThread.Start();
    }

    private void Launch()
    {
        if (Status != ProcessorStatus.Idle)
        {
            throw new Exception("The macro is not able to run.");
        }

        if (Exception is not null)
        {
            throw new Exception("The macro is not able to run.", Exception);
        }

        Status = ProcessorStatus.Launching;
        StartTime = DateTime.Now;
        Timer.Start();

        if (!this.IsIncognitoMode())
        {
            Statistics.Set("last_run_time", StartTime);
            Statistics.Set("total_run_count", (int x) => x + 1);
        }

        ServiceCollection = new();
        ConfigureBasicServices(ServiceCollection);
        ConfigureExtraServices(ServiceCollection);
        ServiceProvider = ServiceCollection.BuildServiceProvider();

        GetService<HookService>();
        GetService<PanelService>();
        var logger = GetService<MacroLogger>();
        logger.Load();
        Logger = new(logger, nameof(MacroProcessor));

        GetService<DashboardService>();

        PrintEnvironments();

        Logger.Info("The macro has launched.");

        if (Exception is not null) // process the exception that occurred in ConfigureExtraServices
        {
            Logger.Error(Exception);
            throw Exception;
        }

        RaiseEvent(ProcessorEvent.Launched, new ProcessorLaunchedEventArgs(StartTime));

        Status = ProcessorStatus.Launched;
    }

    private void Process()
    {
        if (Status != ProcessorStatus.Launched)
        {
            throw new InvalidOperationException();
        }

        Status = ProcessorStatus.Running;

        try
        {
            ProcessStart();

            WorkflowThread = new Thread(ProcessWorkflow);
            WorkflowThread.SetApartmentState(ApartmentState.STA);
            WorkflowThread.Start();
            WorkflowThread.Join();
            WorkflowThread = null;

            ProcessEnd();
        }
        catch (Exception exception)
        {
            Logger?.Error(exception);
            throw;
        }
    }

    private void ProcessStart()
    {
        Logger?.ResetIndent();
        Logger?.Trace("---");

        GetService<HookService>().Raise<ProcessorStartingHook>();

        SetupAutoloadServices();

        SetupPreparations();

        GetService<HookService>().Raise<ProcessorStartedHook>();
    }

    private void PrintEnvironments()
    {
        if (Options.Count > 0)
        {
            Logger?.Trace("User options:", ToLines(Options));
        }

        if (Environments.Count > 0)
        {
            Logger?.Trace("Environments:", ToLines(Environments));
        }

        static string[] ToLines(ParameterValueCollection collection)
        {
            return collection
                .OrderBy(pv => pv.Key)
                .Where(pv => pv.Value is not null)
                .Select(pv => $"- {pv.Key}({pv.Value!.GetType().Name}) = {pv.Value}")
                .ToArray();
        }
    }

    private void ConfigureBasicServices(ServiceCollection services)
    {
        services.AddSingleton(this);

        services.AddOptions<LoggerOptions>();
        services.Configure<LoggerOptions>(options =>
        {
            options.FileLogLevel = Environments.Get<LogLevel>(MacroLogger.FileLogLevelKey);
            options.FrontLogLevel = Environments.Get<LogLevel>(MacroLogger.FrontLogLevelKey);
            var privateFolder = Environments.Get<string>("private_folder");
            options.Filename = privateFolder is null ? null : Path.Combine(privateFolder, "Logs", $"{ProcessId}.log");
        });
        services.AddSingleton<MacroLogger>();

        services.AddSingleton<DashboardService>();
        services.AddTransient<InteractionCallbackArguments>();

        services.AddSingleton<HookService>();
        services.AddSingleton<PanelService>();
        services.AddSingleton<InteractionService>();
        services.AddSingleton<OutputService>();
        services.AddSingleton<DebugService>();
        services.AddSingleton<FileStorageService>();
        services.AddSingleton<LocalStorageService>();

        if (Environments.Get<bool>("is_development"))
        {
            // todo: convert to model
            services.AddSingleton<BackgroundService>();
        }

        services.AddTransient<PanelModel>();
        services.AddTransient<TextInstrument>();
        services.AddTransient<ListInstrument>();
        services.AddTransient<ProgressListInstrument>();
        services.AddTransient<GridInstrument>();
        services.AddTransient<ProgressGridInstrument>();
        services.AddTransient<ImageInstrument>();

        services.AddTransient<FlowBuilderService>();

        services.AddTransient<ArgumentService>();
        services.AddTransient<IterationArguments>();
        services.AddTransient<LoopCheckContinueArguments>();
    }

    private void ConfigureExtraServices(ServiceCollection services)
    {
        // Since the logger service is not ready yet, the exception will be processed afterward.

        foreach (var module in Macro.Modules)
        {
            try
            {
                module.OnProcessorConfigure(this);
            }
            catch (Exception ex)
            {
                Exception = new Exception($"An exception occurred while executing \"{module.Name}.{nameof(module.OnProcessorConfigure)}\".", ex);
                return;
            }
        }

        try
        {
            Macro.Configure(this);
        }
        catch (Exception ex)
        {
            Exception = new Exception($"An exception occurred while executing \"{Macro.GetType().Name}.{nameof(Macro.Configure)}\".", ex);
            return;
        }
    }

    private void SetupAutoloadServices()
    {
        Logger?.Debug("Setting up the auto-loading services.");
        Logger?.IncreaseIndent();

        foreach (var serviceDescriptor in ServiceCollection!)
        {
            var serviceType = serviceDescriptor.ServiceType;
            var isAutoloadable = serviceType.IsAssignableTo(typeof(IAutoloadable));
            if (isAutoloadable)
            {
                Logger?.Trace($"Loading service '{serviceType.Name}'.");
                Logger?.IncreaseIndent();
                GetService(serviceType);
                Logger?.DecreaseIndent();
            }
        }

        Logger?.DecreaseIndent();
    }

    private void SetupPreparations()
    {
        Logger?.Debug("Setting up the preparations.");
        Logger?.IncreaseIndent();

        foreach (var module in Macro.Modules)
        {
            if (module.GetType().GetMethod(nameof(MacroModule.OnProcessorPrepare))?.DeclaringType != module.GetType())
            {
                Logger?.Trace($"'{module.Name}.{nameof(MacroModule.OnProcessorPrepare)}' is not overridden.");
                continue;
            }

            Logger?.Trace($"Executing '{module.Name}.{nameof(MacroModule.OnProcessorPrepare)}'.");
            Logger?.IncreaseIndent();
            module.OnProcessorPrepare(this);
            Logger?.DecreaseIndent();
        }

        Logger?.Trace($"Executing '{nameof(MacroBase)}.{nameof(IBackMacro.Prepare)}'.");
        Logger?.IncreaseIndent();
        Macro.Prepare(this);
        Logger?.DecreaseIndent();

        Logger?.DecreaseIndent();
    }

    private void ProcessEnd()
    {
        Logger?.ResetIndent();
        Logger?.Trace("---");

        EndTime = DateTime.Now;
        Timer.Stop();

        Status = Status switch
        {
            ProcessorStatus.Stopping => ProcessorStatus.Stopped,
            ProcessorStatus.Faulting => ProcessorStatus.Faulted,
            ProcessorStatus.Terminating => ProcessorStatus.Terminated,
            ProcessorStatus.Running => ProcessorStatus.Complete,
            _ => Status // ProcessorStatus.Crushed
        };

        var endReason = Status switch
        {
            ProcessorStatus.Complete => EndReason.Complete,
            ProcessorStatus.Faulted => EndReason.ErrorOccurred,
            ProcessorStatus.Stopped => EndReason.Interrupted,
            ProcessorStatus.Terminated => EndReason.Terminated,
            _ => EndReason.Unknown, // Other cases should not go here.
        };

        var duration = GetElapsedTime();

        var endingHook = new ProcessorEndingHook()
        {
            Reason = endReason,
            StartTime = StartTime,
            Duration = duration,
            CompletionAction = CompletionAction.None,
        };

        try
        {
            GetService<HookService>().Raise(endingHook);
        }
        catch (Exception exception)
        {
            Logger?.Warn(exception.Message);
        }

        var completionAction = endingHook.CompletionAction;
        Comment ??= endingHook.Comment;

        if (!this.IsIncognitoMode())
        {
            Statistics.Set<TimeSpan>("total_run_duration", old => old + duration);
        }

        var history = new ProcessHistoryEntry()
        {
            MacroKey = Macro.Key,
            ProcessId = ProcessId,
            StartTime = StartTime,
            EndTime = EndTime,
            EndReason = endReason,
            Comment = Comment,
            Duration = duration,
        };

        var endedHook = new ProcessorEndedHook()
        {
            HistoryEntry = history,
            CompletionAction = completionAction,
        };

        try
        {
            GetService<HookService>().Raise(endedHook);
        }
        catch (Exception exception)
        {
            Logger?.Warn(exception.Message);
        }

        try
        {
            if (GetService<DashboardService>().IsEmpty)
            {
                var (level, comment) = endReason switch
                {
                    EndReason.Interrupted => (OutputLevel.Attention, Comment ?? "User Stop"),
                    EndReason.ErrorOccurred => (OutputLevel.Failure, Comment ?? "Error Occurred"),
                    EndReason.Complete => (OutputLevel.Success, Comment ?? "Complete"),
                    _ => (OutputLevel.Failure, Comment ?? "Unexpected Error"),
                };
                GetService<OutputService>().Write(level, comment);
            }
        }
        catch (Exception exception)
        {
            Logger?.Warn(exception.Message);
        }

        ClearSessionStorage();

        Logger?.Info(endReason switch
        {
            //EndReason.FailedToLaunch => "The macro has not been initialized successfully.",
            EndReason.Interrupted => "The macro has been stopped by the user.",
            EndReason.ErrorOccurred => "The macro has been terminated due to an error.",
            EndReason.Complete => "The macro has been completed.",
            _ => "The macro has ended due to an unexpected reason.",
        }); // this should be the last log

        SessionStorage.Reset("processor_end_reason", endReason);
        SessionStorage.Reset("processor_history_entry", history);
        SessionStorage.Reset("processor_completion_action", completionAction);
    }

    private void Finally()
    {
        ProcessThread = null;

        var endReason = SessionStorage.Get<EndReason?>("processor_end_reason") ?? EndReason.Crushed;
        var historyEntry = SessionStorage.Get<ProcessHistoryEntry>("processor_history_entry");
        var completionAction = SessionStorage.Get<CompletionAction?>("processor_completion_action") ?? CompletionAction.None;
        
        var args = new ProcessorCompletedEventArgs()
        {
            Reason = endReason,
            HistoryEntry = historyEntry,
            Exception = Exception,
            CompletionAction = completionAction,
        };
        RaiseEvent(ProcessorEvent.Completed, args);
    }

    public void AddStep(WorkflowStep step)
    {
        Steps.Add(step);
    }

    private void ProcessWorkflow()
    {
        if (Status != ProcessorStatus.Running)
        {
            return;
        }

        Logger?.ResetIndent();
        Logger?.Trace("---");

        var initialStepId = GetInitialStepId();
        if (initialStepId is null)
        {
            return;
        }

        Cancellation = new();

        ProcessQueue(initialStepId);
    }

    private string? GetInitialStepId()
    {
        var defaultSteps = Steps.Where(x => x.IsDefault).ToArray();
        if (defaultSteps.Length > 1)
        {
            throw new Exception("More than one default step is set.");
        }

        var initialStepId = defaultSteps.FirstOrDefault()?.Id;
        var getInitialStepIdHook = new GetInitialStepIdHook()
        {
            StepId = initialStepId,
        };
        GetService<HookService>().Raise(getInitialStepIdHook);
        initialStepId = getInitialStepIdHook.StepId;

        if (initialStepId is null)
        {
            Logger?.Debug("Could not find the initial step.");
        }
        else
        {
            Logger?.Trace($"The initial step is set to '{initialStepId}'.");
        }

        return initialStepId;
    }

    private void ProcessQueue(string stepId)
    {
        var nextStepId = stepId;
        WorkflowStepReport? report = null;
        while (true)
        {
            var step = Steps.FirstOrDefault(x => x.Id == nextStepId);
            if (step is null)
            {
                Logger?.Trace($"Workflow step '{nextStepId}' is not found.");
                break;
            }

            report = ProcessStep(step, report);
            Footsteps.Add(step.Id);
            nextStepId = report.NextStepId;

            if (nextStepId is null)
            {
                break;
            }
        }
    }

    private WorkflowStepReport ProcessStep(WorkflowStep step, WorkflowStepReport? previousResult)
    {
        Logger?.ResetIndent();
        Logger?.Trace($"Running workflow step '{step.Id}'.");
        Logger?.IncreaseIndent();

        var startTime = DateTime.Now;
        var startElapsedTime = GetElapsedTime();
        var stepArguments = new WorkflowStepArguments()
        {
            StepId = step.Id,
            PreviousResult = previousResult,
            StartTime = startTime,
            SuccessStepId = step.SuccessStepId,
            FailureStepId = step.FailureStepId,
            ErrorStepId = step.ErrorStepId,
            InterruptionStepId = step.InterruptionStepId,
        };

        WorkflowStepResult stepResult;
        CanAbort = step.IsInterruptable;
        try
        {
            stepResult = step.Action.Invoke(stepArguments) ? WorkflowStepResult.Success : WorkflowStepResult.Failed;
        }
        catch (Exception exception) when (IsInterruptionException(exception))
        {
            stepResult = WorkflowStepResult.Interrupted;
            Logger?.Trace($"The stop request has been performed.");
        }
        catch (Exception exception)
        {
            stepResult = WorkflowStepResult.Error;
            Exception = exception;
            Status = ProcessorStatus.Faulting;
            Logger?.Error(exception);
        }
        CanAbort = false;

        var nextStepId = stepResult switch
        {
            WorkflowStepResult.Failed => stepArguments.FailureStepId ?? stepArguments.SuccessStepId,
            WorkflowStepResult.Error => stepArguments.ErrorStepId ?? stepArguments.FailureStepId ?? stepArguments.SuccessStepId,
            WorkflowStepResult.Interrupted => stepArguments.InterruptionStepId ?? stepArguments.FailureStepId ?? stepArguments.SuccessStepId,
            WorkflowStepResult.Success => stepArguments.SuccessStepId,
            _ => null,
        };

        var endTime = DateTime.Now;
        var endElapsedTime = GetElapsedTime();
        var finallyArguments = new WorkflowStepFinallyArguments()
        {
            StepId = step.Id,
            Result = stepResult,
            StartTime = startTime,
            EndTime = endTime,
            Duration = endElapsedTime - startElapsedTime,
            Output = stepArguments.Output,
            NextStepId = nextStepId,
        };
        try
        {
            step.Finally?.Invoke(finallyArguments);
        }
        catch (Exception exception)
        {
            stepResult = WorkflowStepResult.Error;
            Status = ProcessorStatus.Faulting;
            Logger?.Error(exception);
        }

        var report = new WorkflowStepReport()
        {
            StepId = step.Id,
            Result = stepResult,
            NextStepId = nextStepId,
            Output = stepArguments.Output,
        };

        Logger?.Trace($"Finished running workflow step '{step.Id}'.", new {
            report.StepId,
            report.Result,
            report.NextStepId,
            Output = report.Output?.ToString()
        });
        Logger?.DecreaseIndent();

        return report;

        static bool IsInterruptionException(Exception exception)
        {
            return exception switch
            {
                ThreadInterruptedException or UserAbortException => true,
                WorkflowStoppedException => true,
                TaskCanceledException => true,
                TargetInvocationException tie when tie.InnerException is not null => IsInterruptionException(tie.InnerException),
                _ => false
            };
        }
    }

    private void ClearSessionStorage()
    {
        Logger?.Trace($"Clearing the {nameof(SessionStorage)}.");
        Logger?.IncreaseIndent();

        foreach (var item in SessionStorage)
        {
            if (item.Value is IDisposable idis)
            {
                try
                {
                    idis.Dispose();
                }
                catch (Exception exception)
                {
                    Logger?.Error(exception);
                }
            }
        }
        SessionStorage.Clear();

        Logger?.DecreaseIndent();
    }
}
