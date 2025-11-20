using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.Debugger;
using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Journals;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Components.Storages;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    private readonly List<WorkflowStep> Steps = new();

    private List<string> Footsteps { get; } = new();

    private Thread? ProcessThread;

    private Thread? WorkflowThread;

    private bool CanInterrupt;

    private DateTime StartTime;

    private EndReason EndReason;

    private ProcessorResult? Result;

    private void InternalStart()
    {
        ProcessThread = new Thread(InternalExecute);
        ProcessThread.SetApartmentState(ApartmentState.STA);
        ProcessThread.Start();
    }

    private void InternalExecute()
    {
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
    }

    private void Launch()
    {
        if (Status != ProcessorStatus.Idle)
        {
            throw new Exception("The macro is not able to run.", Exception);
        }

        if (Exception is not null)
        {
            throw new Exception("The macro is not able to run.", Exception);
        }

        Status = ProcessorStatus.Launching;
        StartTime = DateTime.Now;
        Timer.Start();

        Report.Add("macro_key", Macro.Key);
        Report.Add("processor_id", ProcessorId);
        Report.Add("start_time", StartTime);

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

        WorkflowThread = new Thread(ProcessWorkflow);
        WorkflowThread.SetApartmentState(ApartmentState.STA);
        WorkflowThread.Start();
        WorkflowThread.Join();
        WorkflowThread = null;
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

        services.AddSingleton<MacroLogger>();

        services.AddSingleton<DashboardService>();

        services.AddSingleton<HookService>();
        services.AddSingleton<PanelService>();
        services.AddSingleton<InteractionService>();
        services.AddSingleton<OutputService>();
        services.AddSingleton<DebugService>();
        services.AddSingleton<RuntimeStorageService>();
        services.AddSingleton<SessionStorageService>();
        services.AddSingleton<LocalStorageService>();
        services.AddSingleton<GlobalStorageService>();
        services.AddSingleton<JournalService>();

        if (Environments.GetValueOrDefault<bool>("is_development"))
        {
            // todo: convert to model
            services.AddSingleton<BackgroundService>();
        }

        services.AddTransient<PanelModel>();
        services.AddTransient<TextInstrument>();
        services.AddTransient<ListInstrument>();
        services.AddTransient<ProgressListInstrument>();
        services.AddTransient<TileInstrument>();
        services.AddTransient<ProgressTileInstrument>();
        services.AddTransient<ImageInstrument>();
        services.AddTransient<JournalInstrument>();

        services.AddTransient<FlowBuilderService>();

        services.AddTransient<ArgumentService>();
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

        var hookService = GetService<HookService>();

        RegisterHookMethods(Macro);

        foreach (var module in Macro.Modules)
        {
            RegisterHookMethods(module);

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

        void RegisterHookMethods(object obj)
        {
            var objectType = obj.GetType();
            foreach (var method in objectType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                var attr = method.GetCustomAttribute<MacroHookAttribute>();
                if (attr is not null)
                {
                    var methodParameters = method.GetParameters();
                    if (methodParameters.Length != 1)
                    {
                        throw new ArgumentException($"The hook method '{objectType.Name}.{method.Name}' must have exactly one parameter.");
                    }
                    var hookType = methodParameters[0].ParameterType;
                    if (typeof(MacroHook).IsAssignableFrom(hookType) == false)
                    {
                        throw new ArgumentException($"The hook method '{objectType.Name}.{method.Name}' has an invalid parameter type '{hookType.Name}'. It must be a subclass of '{nameof(MacroHook)}'.");
                    }
                    var handlerType = Expression.GetDelegateType([hookType, method.ReturnType]);
                    var handler = method.IsStatic
                        ? Delegate.CreateDelegate(handlerType, method)
                        : method.CreateDelegate(handlerType, obj);
                    hookService.Register(new HookListener(hookType, handler)
                    {
                        Priority = attr.Priority,
                        Once = attr.Once,
                        Subscriber = objectType.Name,
                    });
                }
            }
        }
    }

    private void ProcessEnd()
    {
        Logger?.ResetIndent();
        Logger?.Trace("---");

        var endTime = DateTime.Now;
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

        Report.Add("end_time", endTime);
        Report.Add("run_duration", duration);
        Report.Add("end_reason", endReason);

        var endingHook = new ProcessorEndingHook()
        {
            Reason = endReason,
            StartTime = StartTime,
            EndTime = endTime,
            Duration = duration,
            OutputStorage = OutputStorage,
            Report = Report,
        };

        try
        {
            GetService<HookService>().Raise(endingHook);
        }
        catch (Exception exception)
        {
            Logger?.Warn(exception.Message);
        }

        var endedHook = new ProcessorEndedHook()
        {
            Reason = endReason,
            Report = Report,
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
                var (level, text) = endReason switch
                {
                    EndReason.Interrupted => (OutputLevel.Attention, "User Stop"),
                    EndReason.ErrorOccurred => (OutputLevel.Failure, "Error Occurred"),
                    EndReason.Complete => (OutputLevel.Success, "Complete"),
                    _ => (OutputLevel.Failure, "Unexpected Error"),
                };
                SessionStorage.TryGetValue("processor_comment", out string? comment);
                GetService<OutputService>().Write(level, comment ?? text);
            }
        }
        catch (Exception exception)
        {
            Logger?.Warn(exception.Message);
        }

        Logger?.Info(endReason switch
        {
            //EndReason.FailedToLaunch => "The macro has not been initialized successfully.",
            EndReason.Interrupted => "The macro has been stopped by the user.",
            EndReason.ErrorOccurred => "The macro has been terminated due to an error.",
            EndReason.Complete => "The macro has been completed.",
            _ => "The macro has ended due to an unexpected reason.",
        }); // this should be the last log

        EndReason = endReason;
    }

    private void Finally()
    {
        ProcessThread = null;

        ServiceProvider?.Dispose();
        ServiceProvider = null;

        Result = new ProcessorResult()
        {
            Reason = EndReason,
            Report = new ProcessorReport(Report),
            Output = new ParameterValueCollection(OutputStorage),
            Exception = Exception,
        };

        var args = new ProcessorCompletedEventArgs()
        {
            Reason = EndReason,
            Result = Result,
        };

        RaiseEvent(ProcessorEvent.Completed, args);
    }

    public void AddStep(WorkflowStep step)
    {
        Steps.Add(step);
    }

    private void ProcessWorkflow()
    {
        try
        {
            ProcessStart();

            ProcessMain();

            ProcessEnd();
        }
        catch (Exception exception)
        {
            Exception = exception;
            Status = ProcessorStatus.Faulting;
            Logger?.Critical(exception);
        }
    }

    private void ProcessMain()
    {
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
        do
        {
            var step = Steps.FirstOrDefault(x => x.Id == nextStepId);
            if (step is null)
            {
                throw new Exception($"Workflow step '{nextStepId}' is not found.");
            }

            nextStepId = ProcessStep(step);
        } while (nextStepId is not null);
    }

    private string? ProcessStep(WorkflowStep step)
    {
        Logger?.ResetIndent();
        Logger?.Trace($"Running workflow step '{step.Id}'.");
        Logger?.IncreaseIndent();

        Footsteps.Add(step.Id);

        var startTime = DateTime.Now;
        var startElapsedTime = GetElapsedTime();
        var state = WorkflowStepState.Idle;

        try
        {
            var initiallyArguments = new WorkflowStepInitiallyArguments()
            {
                StepId = step.Id,
                StartTime = startTime,
            };
            step.Initially?.Invoke(initiallyArguments);
            state = WorkflowStepState.InitiallySuccess;
        }
        catch (Exception exception)
        {
            state = WorkflowStepState.InitiallyError;
            Status = ProcessorStatus.Faulting;
            Logger?.Error(exception);
        }

        if (state == WorkflowStepState.InitiallySuccess)
        {
            CanInterrupt = step.IsInterruptable;
            try
            {
                var stepArguments = new WorkflowStepArguments()
                {
                    StepId = step.Id,
                    //PreviousResult = previousResult,
                    StartTime = startTime,
                };
                var isActionSuccess = step.Action.Invoke(stepArguments);
                state = isActionSuccess ? WorkflowStepState.Success : WorkflowStepState.Failed;

                if (Status == ProcessorStatus.Stopping)
                {
                    state = WorkflowStepState.Interrupted;
                    Logger?.Trace($"The stop request has been performed.");
                }
            }
            catch (Exception exception) when (IsInterruptionException(exception))
            {
                state = WorkflowStepState.Interrupted;
                Logger?.Trace($"The stop request has been performed.");
            }
            catch (TargetInvocationException exception)
            {
                state = WorkflowStepState.Error;
                Exception = exception.InnerException;
                Status = ProcessorStatus.Faulting;
                Logger?.Error(exception.InnerException!);
            }
            catch (Exception exception)
            {
                state = WorkflowStepState.Error;
                Exception = exception;
                Status = ProcessorStatus.Faulting;
                Logger?.Error(exception);
            }
            CanInterrupt = false;
        }

        var nextStepId = state switch
        {
            WorkflowStepState.InitiallyError => step.ErrorStepId,
            WorkflowStepState.Failed => step.FailureStepId,
            WorkflowStepState.Error => step.ErrorStepId,
            WorkflowStepState.Success => step.SuccessStepId,
            _ => null,
        };

        try
        {
            var endTime = DateTime.Now;
            var endElapsedTime = GetElapsedTime();
            var finallyArguments = new WorkflowStepFinallyArguments()
            {
                StepId = step.Id,
                State = state,
                StartTime = startTime,
                EndTime = endTime,
                Duration = endElapsedTime - startElapsedTime,
                NextStepId = nextStepId,
            };

            step.Finally?.Invoke(finallyArguments);
            nextStepId = finallyArguments.NextStepId;
        }
        catch (Exception exception)
        {
            // to avoid confusion, the finally phase should not throw any exception
            // if it does, stop the workflow immediately
            Status = ProcessorStatus.Faulting;
            Logger?.Error(exception);
            return null;
        }

        Logger?.Trace($"Finished running workflow step '{step.Id}'.", new {
            state,
            nextStepId,
        });
        Logger?.DecreaseIndent();

        return nextStepId;

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

}
