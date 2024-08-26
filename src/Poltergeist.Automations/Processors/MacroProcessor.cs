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

public sealed class MacroProcessor : IFrontProcessor, IServiceProcessor, IConfigurableProcessor, IUserProcessor, IPreparableProcessor, IDisposable
{
    public event EventHandler<ProcessorLaunchedEventArgs>? Launched;
    public event EventHandler<ProcessorCompletedEventArgs>? Completed;
    public event EventHandler<PanelCreatedEventArgs>? PanelCreated;
    public event EventHandler<InteractingEventArgs>? Interacting;

    public string ProcessId { get; }
    public string? ShellKey { get; }

    private IBackMacro Macro { get; }
    IUserMacro IUserProcessor.Macro => (IUserMacro)Macro;
    IUserMacro IPreparableProcessor.Macro => (IUserMacro)Macro;
    IFrontMacro IFrontProcessor.Macro => (IFrontMacro)Macro;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public ServiceCollection? ServiceCollection { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }
    ServiceCollection IConfigurableProcessor.Services => ServiceCollection!;

    public bool IsCancelled { get; set; }

    public ParameterValueCollection Options { get; } = new();
    public ParameterValueCollection Environments { get; } = new();
    public ParameterValueCollection Statistics { get; } = new();
    public ParameterValueCollection SessionStorage { get; } = new();

    private HookService? Hooks { get; set; }

    private PauseProvider? PauseProvider;

    public LaunchReason Reason { get; set; }

    private readonly SynchronizationContext? OriginalContext = SynchronizationContext.Current;

    public Action? WorkProc { get; set; }
    private Thread? WorkingThread;

    public Func<Task>? AsyncWorkProc { get; set; }
    private Func<Task>? WorkingTask;

    private bool CanAbort { get; set; }

    private CancellationTokenSource? Cancellation { get; set; }
    CancellationToken? IUserProcessor.CancellationToken => Cancellation?.Token;

    public string? Comment { get; set; }

    public Exception? Exception { get; set; }

    private bool IsDisposed;

    public MacroProcessor(IFrontBackMacro macro, LaunchReason reason)
    {
        Macro = (IBackMacro)macro;
        Reason = reason;
        ProcessId = Guid.NewGuid().ToString();

        Macro.Initialize();

        var invalidationMessage = Macro.CheckValidity();
        if (invalidationMessage is not null)
        {
            Exception = new Exception(invalidationMessage);
            return;
        }
    }

    public MacroProcessor(IFrontBackMacro macro, LaunchReason reason, string shellKey) : this(macro, reason)
    {
        ShellKey = shellKey;
    }

    public T GetService<T>() where T : class
    {
        return (T)GetService(typeof(T))!;
    }

    public object? GetService(Type type)
    {
        return ServiceProvider!.GetService(type);
    }

    #region "Workflow"

    public void Launch()
    {
        if (Exception is not null)
        {
            throw new InvalidOperationException("The macro is not able to run.", Exception);
        }

        StartTime = DateTime.Now;
        Statistics.Set("last_run_time", StartTime);
        Statistics.Set("total_run_count", (int x) => x + 1);

        RaiseEvent(ProcessorEvent.Launched, new ProcessorLaunchedEventArgs(StartTime));

        Load();
        if (Exception is not null)
        {
            End(EndReason.LoadFailed);
            return;
        }

        if (!CheckStart())
        {
            End(EndReason.Unstarted);
            return;
        }

        Start();
    }

    private void Load()
    {
        ServiceCollection = new();
        ConfigureBasicServices(ServiceCollection);
        ConfigureExtraServices(ServiceCollection);
        ServiceProvider = ServiceCollection.BuildServiceProvider();

        // active basic services
        Hooks = GetService<HookService>();
        GetService<PanelService>();
        GetService<MacroLogger>().Load();
        GetService<DashboardService>();

        if (Exception is not null)
        {
            return;
        }

        LoadStartupServices();
        if (Exception is not null)
        {
            return;
        }

        Prepare();
        if (Exception is not null)
        {
            return;
        }

        SetWork();
        if (Exception is not null)
        {
            return;
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
        services.AddTransient<LoopBuilderService>();
        services.AddTransient<LoopExecuteArguments>();
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

    private void LoadStartupServices()
    {
        Log(LogLevel.Debug, "Started activating the startup services.");

        foreach (var serviceDescriptor in ServiceCollection!)
        {
            var serviceType = serviceDescriptor.ServiceType;
            var isAutoloadable = serviceType.IsAssignableTo(typeof(IAutoloadable));
            if (isAutoloadable)
            {
                try
                {
                    GetService(serviceType);
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"Failed to activate service <{serviceType.Name}> services.");
                    Exception = ex;
                    return;
                }
            }
        }

        Log(LogLevel.Debug, "Finished activating the startup services.");
    }

    private void Prepare()
    {
        Log(LogLevel.Debug, "Started running the preparation procedure.");

        foreach (var module in Macro.Modules)
        {
            try
            {
                module.OnProcessorPrepare(this);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"An exception occurred while executing \"{module.Name}.{nameof(module.OnProcessorPrepare)}\".");
                Exception = ex;
                return;
            }
        }

        try
        {
            Macro.Prepare(this);
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"An exception occurred while executing \"{Macro.GetType().Name}.{nameof(Macro.Prepare)}\".");
            Exception = ex;
            return;
        }

        try
        {
            Hooks?.Raise<ProcessorPreparedHook>();
        }
        catch (Exception ex)
        {
            Exception = ex;
            return;
        }

        Log(LogLevel.Debug, "Finished running the preparation procedure.");
    }

    private void SetWork()
    {
        if (WorkProc is not null)
        {
            Log(LogLevel.Debug, $"<{nameof(WorkProc)}> is set. The workflow will be run in thread mode.");

            var start = new ThreadStart(ProcessThreadWorkflow);
            WorkingThread = new Thread(start);
            WorkingThread.SetApartmentState(ApartmentState.STA);
        }
        else if (AsyncWorkProc is not null)
        {
            Log(LogLevel.Debug, $"<{nameof(AsyncWorkProc)}> is set. The workflow will be run in task mode.");

            WorkingTask = ProcessTaskWorkflow;
        }
        else
        {
            Log(LogLevel.Debug, $"Neither <{nameof(WorkProc)}> nor <{nameof(AsyncWorkProc)}> is set.");
        }
    }

    private bool CheckStart()
    {
        Log(LogLevel.Debug, "Started running the check-start procedure.");

        var checkHook = new ProcessorCheckStartHook()
        {
            CanStart = true,
        };

        try
        {
            Hooks?.RaiseUntil(checkHook, x => !x.CanStart);
        }
        catch (Exception ex)
        {
            Exception = ex;
            return false;
        }

        if (!checkHook.CanStart)
        {
            Log(LogLevel.Debug, "The start check is not passed."); // not an error
            return false;
        }

        Log(LogLevel.Debug, $"Finished running the check-start procedure.");

        return true;
    }

    private void Start()
    {
        try
        {
            Hooks?.Raise<ProcessorStartingHook>();
        }
        catch (Exception ex)
        {
            Exception = ex;
            End(EndReason.ErrorOccurred);
            return;
        }

        try
        {
            Hooks?.Raise<ProcessorStartedHook>();
        }
        catch (Exception ex)
        {
            Exception = ex;
            End(EndReason.ErrorOccurred);
            return;
        }

        if (WorkingThread is not null)
        {
            WorkingThread.Start();
        }
        else if (WorkingTask is not null)
        {
            WorkingTask();
        }
        else
        {
            End(EndReason.Empty);
        }
    }

    private void ProcessThreadWorkflow()
    {
        Log(LogLevel.Debug, "Started running the working procedure in thread mode.");

        CanAbort = true;

        try
        {
            WorkProc!.Invoke();
        }
        catch (UserAbortException)
        {
            Log(LogLevel.Debug, "The workflow is interrupted due to user aborting.");
            End(EndReason.UserAborted);
            return;
        }
        catch (ThreadInterruptedException) when (IsCancelled)
        {
            Log(LogLevel.Debug, "The workflow is interrupted due to user aborting.");
            End(EndReason.UserAborted);
            return;
        }
        catch (AggregateException ex) when (ex.InnerException is ThreadInterruptedException && IsCancelled)
        {
            Log(LogLevel.Debug, "The workflow is interrupted due to user aborting.");
            End(EndReason.UserAborted);
            return;
        }
        catch (Exception ex)
        {
            Exception = ex;
            End(EndReason.ErrorOccurred);
            return;
        }
        finally
        {
            CanAbort = false;
        }

        Log(LogLevel.Debug, "Finished running the working procedure.");

        End(EndReason.Complete);
    }

    private async Task ProcessTaskWorkflow()
    {
        Log(LogLevel.Debug, "Started running the working procedure in task mode.");

        Cancellation = new();
        CanAbort = true;

        try
        {
            await AsyncWorkProc!();
        }
        catch (UserAbortException)
        {
            Log(LogLevel.Debug, "The workflow is interrupted due to user aborting.");
            End(EndReason.UserAborted);
            return;
        }
        catch (ThreadInterruptedException)
        {
            Log(LogLevel.Debug, "The workflow is interrupted due to user aborting.");
            End(EndReason.UserAborted);
            return;
        }
        catch (Exception ex)
        {
            Exception = ex;
            End(EndReason.ErrorOccurred);
            return;
        }
        finally
        {
            CanAbort = false;
        }

        Log(LogLevel.Debug, "Finished running the working procedure.");

        End(EndReason.Complete);
    }

    private void End(EndReason reason)
    {
        if (Exception is not null)
        {
            Log(Exception);
        }

        Log(LogLevel.Debug, "Started running the ending procedure.");

        EndTime = DateTime.Now;
        var duration = EndTime - StartTime;

        var endingHook = new ProcessorEndingHook()
        {
            Reason = reason,
            StartTime = StartTime,
            Duration = duration,
            CompletionAction = CompletionAction.None,
        };

        try
        {
            Hooks!.Raise(endingHook);
        }
        catch (Exception exception)
        {
            Log(exception, LogLevel.Warning);
        }

        Comment ??= endingHook.Comment;

        Statistics.Set<TimeSpan>("total_run_duration", old => old + duration);

        var history = new ProcessHistoryEntry()
        {
            MacroKey = Macro.Key,
            ProcessId = ProcessId,
            StartTime = StartTime,
            EndTime = EndTime,
            EndReason = reason,
            Comment = Comment,
        };

        var endedHook = new ProcessorEndedHook()
        {
            HistoryEntry = history,
            CompletionAction = endingHook.CompletionAction,
        };

        try
        {
            Hooks!.Raise(endedHook);
        }
        catch (Exception exception)
        {
            Log(exception, LogLevel.Warning);
        }

        var args = new ProcessorCompletedEventArgs()
        {
            Status = reason,
            HistoryEntry = history,
            Exception = Exception,
            CompletionAction = endingHook.CompletionAction,
        };

        RaiseEvent(ProcessorEvent.Completed, args);

        Log(LogLevel.Information, $"The workflow is ended: {reason}.");

        Log(LogLevel.Debug, "The processor will shut down."); // this should be the last log

        Dispose();
    }

    #endregion

    public void Abort() // todo: add reason
    {
        IsCancelled = true;

        if (!CanAbort)
        {
            return;
        }

        Log(LogLevel.Information, "User aborting.");

        WorkingThread?.Interrupt();

        Cancellation?.Cancel();

        if (PauseProvider?.IsPaused == true)
        {
            PauseProvider.Resume();
        }
    }

    private void Log(LogLevel level, string message)
    {
        GetService<MacroLogger>().Log(level, nameof(MacroProcessor), message);
    }

    private void Log(Exception exception, LogLevel level = LogLevel.Error)
    {
        Log(level, exception.Message);

        if (exception.InnerException is not null)
        {
            Log(exception.InnerException, level);
        }
    }

    public void RaiseEvent(ProcessorEvent type, EventArgs eventArgs)
    {
        MulticastDelegate? multicastDelegate = type switch
        {
            ProcessorEvent.Launched => Launched,
            ProcessorEvent.Completed => Completed,
            ProcessorEvent.PanelCreated => PanelCreated,
            ProcessorEvent.Interacting => Interacting,
            _ => throw new NotSupportedException(),
        };
        if (multicastDelegate is null)
        {
            return;
        }

        var handlerArgs = new object[] { this, eventArgs };

        OriginalContext!.Post(d =>
        {
            multicastDelegate?.DynamicInvoke(handlerArgs);
        }, null);
    }

    public void RaiseAction(Action action)
    {
        OriginalContext!.Post(d =>
        {
            action.DynamicInvoke();
        }, null);
    }

    public async Task Pause()
    {
        Log(LogLevel.Debug, "The processor is paused.");

        PauseProvider = new();
        await PauseProvider.Pause();
        PauseProvider = null;

        if (IsCancelled)
        {
            throw new UserAbortException();
        }
    }

    public void Resume()
    {
        if (PauseProvider is null)
        {
            return;
        }

        Log(LogLevel.Debug, "The processor is resumed.");

        PauseProvider.Resume();
    }

    public void ReceiveMessage(Dictionary<string, string> paramaters)
    {
        Hooks?.Raise(new MessageReceivedHook(paramaters));
    }


    public void CheckCancel()
    {
        Cancellation?.Token.ThrowIfCancellationRequested();
    }


    public void Interact(InteractionModel model)
    {
        var args = new InteractingEventArgs(model);
        RaiseEvent(ProcessorEvent.Interacting, args);
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            ServiceProvider?.Dispose();

            foreach (var item in SessionStorage)
            {
                if (item.Value is IDisposable idis)
                {
                    idis.Dispose();
                }
            }
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
