using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Journals;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Components.Storages;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    private Task<ProcessorResult>? WorkflowTask;

    private readonly List<MacroModule> Modules = new();

    private async Task<ProcessorResult> ProcessAsync()
    {
        try
        {
            Initialize();

            Launch();

            Startup();

            var conclusion = await RunAsync();

            Shutdown(conclusion);
        }
        catch (Exception exception)
        {
            Exception = exception;
            Status = ProcessorStatus.Crushed;
            Report.Add(ProcessorResult.ConclusionDefinition, ProcessorConclusion.Crushed);
            Log(LogLevel.Critical, Exception.Message);
        }

        var result = new ProcessorResult()
        {
            Report = new ProcessorReport(Report),
            Outputs = new ParameterValueCollection(OutputStorage),
            Exception = Exception,
        };

        var args = new ProcessorCompletedEventArgs()
        {
            Result = result,
        };

        RaiseEvent(ProcessorEvent.Completed, args);

        return result;
    }

    private void Initialize()
    {
        var startTime = DateTime.Now;
        Timer.Start();

        Report.Add(ProcessorResult.MacroKeyDefinition, Macro.Key);
        Report.Add(ProcessorResult.ProcessorIdDefinition, ProcessorId);
        Report.Add(ProcessorResult.StartTimeDefinition, startTime);
    }

    private void Launch()
    {
        Status = ProcessorStatus.Launching;

        Modules.AddRange(Macro.Modules.Where(x => x.Validate(this)));

        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        Status = ProcessorStatus.Launched;

        RaiseEvent(ProcessorEvent.Launched, new ProcessorLaunchedEventArgs());
    }

    private void LoadKernelServices()
    {
        GetService<HookService>();
        GetService<PanelService>();
        var logger = GetService<MacroLogger>();
        logger.Load();
        Logger = new(logger, nameof(MacroProcessor));

        GetService<DashboardService>();

        PrintEnvironments();

        Logger.Info("The macro has launched.");
    }

    private async Task<ProcessorConclusion> RunAsync()
    {
        Logger?.ResetIndent();
        Logger?.Trace("---");

        Status = ProcessorStatus.Running;

        ProcessorConclusion conclusion;

        if (TryGetService<IWorkflowExecutor>(out var executor))
        {
            try
            {
                var isSuccess = await executor.Process();
                conclusion = isSuccess ? ProcessorConclusion.Success : ProcessorConclusion.Failure;
            }
            catch (Exception exception) when (IsStopException(exception))
            {
                Status = ProcessorStatus.Stopping;
                Logger?.Trace($"The stop request has been performed.");
                conclusion = ProcessorConclusion.Canceled;
            }
            catch (Exception exception)
            {
                Status = ProcessorStatus.Faulting;
                Exception = exception;
                conclusion = ProcessorConclusion.ErrorOccurred;
            }

            executor.Cleanup();
        }
        else
        {
            conclusion = ProcessorConclusion.Success;
        }

        return conclusion;
    }

    private void Shutdown(ProcessorConclusion conclusion)
    {
        Logger?.ResetIndent();
        Logger?.Trace("---");

        Status = Status switch
        {
            ProcessorStatus.Running => ProcessorStatus.Complete,
            ProcessorStatus.Stopping => ProcessorStatus.Stopped,
            ProcessorStatus.Faulting => ProcessorStatus.Faulted,
            ProcessorStatus.Terminating => ProcessorStatus.Terminated,
            _ => throw new UnreachableException()
        };

        Timer.Stop();
        var startTime = Report.Get(ProcessorResult.StartTimeDefinition);
        var endTime = DateTime.Now;
        var duration = GetElapsedTime();
        Report.Add(ProcessorResult.EndTimeDefinition, endTime);
        Report.Add(ProcessorResult.DurationDefinition, duration);
        Report.Add(ProcessorResult.ConclusionDefinition, conclusion);

        var endingHook = new ProcessorCompletedHook()
        {
            Conclusion = conclusion,
            StartTime = startTime,
            EndTime = endTime,
            Duration = duration,
            OutputStorage = OutputStorage,
            Report = Report,
        };
        GetService<HookService>().Raise(endingHook);
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

    private void RegisterServices(ServiceCollection services)
    {
        services.AddSingleton(this);

        services.AddSingleton<HookService>();
        services.AddSingleton<MacroLogger>();

        services.AddSingleton<InteractionService>();
        services.AddSingleton<OutputService>();
        services.AddSingleton<RuntimeStorageService>();
        services.AddSingleton<SessionStorageService>();
        services.AddSingleton<LocalStorageService>();
        services.AddSingleton<GlobalStorageService>();

        services.AddTransient<WorkflowController>();

        services.AddSingleton<PanelService>();
        services.AddSingleton<DashboardService>();
        services.AddSingleton<JournalService>();
        services.AddTransient<PanelModel>();
        services.AddTransient<TextInstrument>();
        services.AddTransient<ListInstrument>();
        services.AddTransient<ProgressListInstrument>();
        services.AddTransient<TileInstrument>();
        services.AddTransient<ProgressTileInstrument>();
        services.AddTransient<ImageInstrument>();
        services.AddTransient<JournalInstrument>();
        services.AddTransient<IndicatorInstrument>();
        services.AddSingleton<LabelService>();
        services.AddTransient<LabelInstrument>();

        if (Environments.GetValueOrDefault<bool>("is_development"))
        {
            // todo: convert to model
            services.AddSingleton<BackgroundService>();
        }
        
        services.AddTransient<FlowBuilderService>();

        {
            var registerServicesArguments = new RegisterServicesArguments()
            {
                Environments = Environments,
                Options = Options,
            };

            Macro.RegisterServices(services, registerServicesArguments);
            RegisterServiceDependencies(services, Macro);

            foreach (var module in Modules)
            {
                RegisterServiceDependencies(services, module);
                module.RegisterServices(services, registerServicesArguments);
            }
        }

        static void RegisterServiceDependencies(ServiceCollection services, object obj)
        {
            var objectType = obj.GetType();
            var dependencyAttributes = objectType.GetCustomAttributes(typeof(ServiceDependencyAttribute<>));
            foreach (var dependencyAttribute in dependencyAttributes)
            {
                var serviceType = dependencyAttribute.GetType().GetGenericArguments()[0];
                var lifetime = ((IServiceDependency)dependencyAttribute).Lifetime;
                if (lifetime == Services.ServiceLifetime.Singleton)
                {
                    services.AddSingleton(serviceType);
                }
                else if (lifetime == Services.ServiceLifetime.Transient)
                {
                    services.AddTransient(serviceType);
                }
                else if (lifetime == Services.ServiceLifetime.Scoped)
                {
                    services.AddScoped(serviceType);
                }
            }
        }
    }

    private void Startup()
    {
        Logger?.ResetIndent();
        Logger?.Trace("---");

        LoadKernelServices();

        RegisterHookMethods();

        GetService<HookService>().Raise<ProcessorStartupHook>();
    }


    private void RegisterHookMethods()
    {
        Logger?.Debug("Registering hook methods.");
        Logger?.IncreaseIndent();

        var hookService = GetService<HookService>();

        hookService.RegisterMethods(Macro);

        foreach (var module in Modules)
        {
            hookService.RegisterMethods(module);
        }

        Logger?.DecreaseIndent();
    }

    void IMacroProcessorInternal.ReportException(Exception exception)
    {
        Exception = exception;
        Status = ProcessorStatus.Faulting;
        //Logger?.Error(Exception);
    }

    void IMacroProcessorShared.ReportComment(string comment)
    {
        Report.Add(ProcessorResult.CommentDefinition, comment);
    }

    public static bool IsStopException(Exception exception)
    {
        return exception switch
        {
            WorkflowCanceledException => true,
            TaskCanceledException => true,
            TargetInvocationException tie when tie.InnerException is not null => IsStopException(tie.InnerException),
            _ => false
        };
    }
}
