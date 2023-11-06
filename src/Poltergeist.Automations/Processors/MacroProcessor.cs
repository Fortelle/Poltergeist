using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Components.Repetitions;
using Poltergeist.Automations.Exceptions;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors.Events;

namespace Poltergeist.Automations.Processors;

public sealed class MacroProcessor : IServiceProcessor, IConfigureProcessor, IUserProcessor, IDisposable
{
    public event EventHandler<MacroStartingEventArgs>? Starting;
    public event EventHandler<MacroStartedEventArgs>? Started;
    public event EventHandler<MacroCompletedEventArgs>? Completed;
    public event EventHandler<PanelCreatedEventArgs>? PanelCreated;
    public event EventHandler<InteractingEventArgs>? Interacting;

    public string ProcessId { get; set; }

    public IMacroBase Macro { get; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public ServiceCollection? ServiceCollection { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }

    public bool IsCancelled { get; set; }

    public required Dictionary<string, object?> Options { get; set; }
    public required Dictionary<string, object?> Environments { get; set; }

    private HookService? Hooks { get; set; }
    private WorkingService? Workflow { get; set; }

    private PauseProvider? PauseProvider;

    internal Exception? InitializationException;

    public LaunchReason Reason { get; set; }

    public CacheStorage Caches { get; } = new();

    private readonly SynchronizationContext? OriginalContext = SynchronizationContext.Current;

    CancellationToken? IUserProcessor.CancellationToken => Workflow?.GetCancellationToken();

    public string? Summary { get; set; }

    public MacroProcessor(IMacroBase data, LaunchReason reason)
    {
        ProcessId = Guid.NewGuid().ToString();

        Macro = data;
        Reason = reason;

        InitializeMacroData();
    }

    private void InitializeMacroData()
    {
        try
        {
            Macro.Initialize();

            if (Macro.RequiresAdmin)
            {
                CheckAdmin();
            }
        }
        catch (Exception e)
        {
            InitializationException = e;
        }
    }

    private void InitializeProcessor()
    {
        Options ??= Macro.UserOptions.ToDictionary();
        Environments ??= new();

        ServiceCollection = new();
        InitializeBasicServices(ServiceCollection);
        InitializeExtraServices(ServiceCollection);
        ServiceProvider = ServiceCollection.BuildServiceProvider();

        // active basic services
        Hooks = GetService<HookService>();
        GetService<PanelService>();
        GetService<MacroLogger>();

        GetService<DashboardService>();

        Workflow = GetService<WorkingService>();
        Workflow.Ending += OnEnding;
        Workflow.Ended += OnEnded;
    }

    private void InitializeBasicServices(ServiceCollection services)
    {
        services.AddSingleton(this);

        services.AddOptions<LoggerOptions>();
        services.Configure<LoggerOptions>(options =>
        {
            options.FileLogLevel = GetEnvironment("logger.tofile", LogLevel.All);
            options.FrontLogLevel = GetEnvironment("logger.toconsole", LogLevel.All);
            options.Filename = Macro.PrivateFolder is null ? null : Path.Combine(Macro.PrivateFolder, "Logs", $"{ProcessId}.log");
        });
        services.AddSingleton<MacroLogger>();

        services.AddSingleton<DashboardService>();
        services.AddTransient<InteractionCallbackArguments>();

        services.AddSingleton<WorkingService>();
        services.AddSingleton<HookService>();
        services.AddSingleton<PanelService>();
        services.AddSingleton<InteractionService>();
        services.AddSingleton<OutputService>();
        services.AddSingleton<DebugService>();

        if (Debugger.IsAttached)
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

        services.AddTransient<ArgumentService>();


        services.AddTransient<FlowBuilderService>();
        services.AddTransient<LoopBuilderService>();
        services.AddTransient<LoopBeforeArguments>();
        services.AddTransient<LoopExecutionArguments>();
        services.AddTransient<LoopCheckContinueArguments>();
    }

    private void InitializeExtraServices(ServiceCollection services)
    {
        if (InitializationException != null)
        {
            return;
        }

        try
        {
            foreach (var module in Macro.Modules)
            {
                module.OnMacroConfiguring(services, this);
            }

            Macro.ConfigureServices(services, this);
        }
        catch (Exception e)
        {
            InitializationException = e;
        }
    }

    private void CheckAdmin()
    {
        if (!Macro.RequiresAdmin)
        {
            return;
        }

        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        if (!isAdmin)
        {
            throw new Exception("This macro requires administrator privileges to run.");
        }
    }

    public T? GetOption<T>(string key, T? def = default)
    {
        var value = GetOption(key, typeof(T));

        if (value is null)
        {
            return def;
        }

        return (T)value;
    }

    public object? GetOption(string key, Type type)
    {
        if (Options.TryGetValue(key, out var value))
        {
            if (value is JToken jt)
            {
                return jt.ToObject(type)!;
            }
            else
            {
                return value;
            }
        }
        else
        {
            return null;
        }
    }

    void IConfigureProcessor.SetOption(string key, object value)
    {
        if (Options.ContainsKey(key))
        {
            Options[key] = value;
        }
        else
        {
            Options.Add(key, value);
        }
    }

    public T? GetEnvironment<T>(string key, T? def = default)
    {
        if (Environments.TryGetValue(key, out var value))
        {
            return (T?)value;
        }
        else
        {
            return def;
        }
    }

    public void SetStatistic<T>(string key, T value) where T : notnull
    {
        if (!GetEnvironment<bool>("macro.usestatistics"))
        {
            return;
        }

        Macro.Statistics.Set(key, value!);
        //Log(LogLevel.Debug, $"Set statistic {key} = {value}.");
    }

    public void SetStatistic<T>(string key, Func<T, T> action) where T : notnull
    {
        if (!GetEnvironment<bool>("macro.usestatistics"))
        {
            return;
        }

        Macro.Statistics.Set(key, action);
        //Log(LogLevel.Debug, $"Set statistic {key} = {newValue}.");
    }

    public T GetStatistic<T>(string key) where T : notnull
    {
        var item = Macro.Statistics.FirstOrDefault(x => x.Key == key);
        if (item is null)
        {
            throw new KeyNotFoundException();
        }

        return (T)item.Value;
    }

    public T GetService<T>() where T : class
    {
        return (T)GetService(typeof(T))!;
    }

    public object? GetService(Type type)
    {
        return ServiceProvider!.GetService(type);
    }

    public void Launch()
    {
        StartTime = DateTime.Now;

        InitializeProcessor();

        Log(LogLevel.Information, "The macro has started up.");

        RaiseEvent(MacroEventType.ProcessStarting, new MacroStartingEventArgs(StartTime));

        if (InitializationException == null)
        {
            foreach (var module in Macro.Modules)
            {
                module.OnMacroProcessing(this);
            }
            Macro.Process(this);
        }

        SetStatistic<int>("total_run_count", old => old + 1);
        SetStatistic("last_run_time", StartTime);

        Workflow!.Start();
    }

    public void Abort()
    {
        IsCancelled = true;

        Workflow?.Abort();

        if (PauseProvider?.IsPaused == true)
        {
            PauseProvider.Resume();
        }
    }

    private void Log(LogLevel level, string message)
    {
        GetService<MacroLogger>().Log(level, nameof(MacroProcessor), message);
    }

    private void OnEnding(object? sender, WorkEndingEventArgs e)
    {
        EndTime = DateTime.Now;
        var duration = EndTime - StartTime;

        SetStatistic<TimeSpan>("total_run_duration", old => old + duration);

        var completeAction = GetOption("aftercompletion.action", CompletionAction.None);
        if (e.Reason == EndReason.UserAborted)
        {
            completeAction = CompletionAction.None;
        }

        var completeAllowerror = GetOption("aftercompletion.allowerror", false);
        if (!completeAllowerror && e.Reason != EndReason.Complete)
        {
            completeAction = CompletionAction.None;
        }

        var completeMinimumSeconds = GetOption("aftercompletion.minimumseconds", 0);
        if (completeMinimumSeconds > 0 && completeMinimumSeconds < duration.TotalSeconds)
        {
            completeAction = CompletionAction.None;
        }

        if (completeAction == CompletionAction.None)
        {
            if (Macro.MinimizeApplication)
            {
                completeAction = CompletionAction.RestoreApplication;
            }
        }

        Macro.SaveStatistics();

        var summary = new ProcessSummary()
        {
            MacroKey = Macro.Name,
            ProcessId = ProcessId,
            StartTime = StartTime,
            EndTime = EndTime,
            EndReason = e.Reason,
            Comment = Summary,
        };
        Hooks?.Raise(new ProcessSummaryCreatedHook(summary));

        var args = new MacroCompletedEventArgs(e.Reason, summary)
        {
             CompleteAction = completeAction,
        };

        Macro.Summaries.Add(summary);
        Macro.SaveSummaries();

        RaiseEvent(MacroEventType.ProcessCompleted, args);
    }

    private void OnEnded(object? sender, EventArgs e)
    {
        Dispose();
    }

    public void RaiseEvent(MacroEventType type, EventArgs eventArgs)
    {
        MulticastDelegate? multicastDelegate = type switch
        {
            MacroEventType.ProcessStarting => Starting,
            MacroEventType.ProcessStarted => Started,
            MacroEventType.ProcessCompleted => Completed,
            MacroEventType.PanelCreated => PanelCreated,
            MacroEventType.Interacting => Interacting,
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
        Log(LogLevel.Debug, "The process is paused.");

        PauseProvider = new();
        await PauseProvider.Pause();
        PauseProvider = null;

        if (Workflow!.IsAborted)
        {
            throw new UserAbortException();
        }
    }

    public void Resume()
    {
        if(PauseProvider is null)
        {
            return;
        }

        Log(LogLevel.Debug, "The process is resumed.");

        PauseProvider.Resume();
    }

    public void ReceiveMessage(Dictionary<string, string> paramaters)
    {
        Hooks?.Raise(new MessageReceivedHook(paramaters));
    }


    public void Dispose()
    {
        ServiceProvider!.Dispose();

        foreach(var item in Caches.GetAll<IDisposable>())
        {
            item.Dispose();
        }
    }
}
