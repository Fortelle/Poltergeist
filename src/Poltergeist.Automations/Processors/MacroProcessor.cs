using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Panels;
using Poltergeist.Automations.Processors.Events;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Processors;

public sealed class MacroProcessor : IDisposable
{
    public event EventHandler<MacroStartedEventArgs> Starting;
    public event EventHandler Started;
    public event EventHandler<MacroCompletedEventArgs> Completed;
    public event EventHandler<PanelCreatedEventArgs> PanelCreated;

    public string ProcessId { get; set; }

    public IMacroBase Macro { get; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public HookService Hooks { get; set; }
    public WorkingService Workflow { get; set; }

    public ServiceProvider Services { get; set; }

    public bool WaitUiReady { get; set; }

    public Dictionary<string, object> Options { get; set; }
    public Dictionary<string, object> Environments { get; set; }

    private PauseTokenSource PauseTokenSource;
    private PauseToken PauseToken;

    internal Exception InitializationException;
    internal Type[] AutoloadTypes;
    internal string[] ServiceList;

    public LaunchReason Reason;

    private readonly SynchronizationContext originalContext = SynchronizationContext.Current;

    private MacroProcessor()
    {
        ProcessId = Guid.NewGuid().ToString();
        //Dispatcher = DispatcherQueue.GetForCurrentThread();
    }

    public MacroProcessor(IMacroBase data, LaunchReason reason) : this()
    {
        Macro = data;
        Reason = reason;

        InitializeMacroData();

        //Options = Macro.UserOptions.ToDictionary();

        //InitializeProcessor();
    }

    //public MacroProcessor(IMacroBase data, LaunchReason reason, Dictionary<string, object> options) : this()
    //{
    //    Macro = data;
    //    Reason = reason;

    //    InitializeMacroData();

    //    Options = options;

    //    InitializeProcessor();
    //}

    private void InitializeMacroData()
    {
        try
        {
            Macro.Initialize();

            if (Macro.RequireAdmin)
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

        var services = new MacroServiceCollection();
        InitializeBasicServices(services);
        InitializeExtraServices(services);
        AutoloadTypes = services.AutoloadTypes.ToArray();
        Services = services.BuildServiceProvider();
        ServiceList = services.Select(x => x.ServiceType.Name).ToArray();

        // active basic services
        GetService<MacroLogger>(); // first
        Hooks = GetService<HookService>(); // second
        GetService<PanelService>();
        GetService<InstrumentService>();

        Workflow = GetService<WorkingService>();
        Workflow.WaitUI = true;
        Workflow.Ending += OnEnd;
    }

    private void InitializeBasicServices(MacroServiceCollection services)
    {
        services.AddSingleton(this);

        services.AddOptions<LoggerOptions>();
        services.Configure<LoggerOptions>(options =>
        {
            options.FileLogLevel = GetEnvironment("logger.tofile", LogLevel.All);
            options.FrontLogLevel = GetEnvironment("logger.toconsole", LogLevel.All);
            options.Filename = Path.Combine(Macro.PrivateFolder, "Logs", $"{ProcessId}.log");
        });
        services.AddSingleton<MacroLogger>();

        services.AddSingleton<InstrumentService>();

        services.AddSingleton<WorkingService>();
        services.AddSingleton<HookService>();
        services.AddSingleton<PanelService>();

        services.AddTransient<DialogService>();

        if (Debugger.IsAttached)
        {
            services.AddAutoload<Components.Background.BackgroundService>();
        }

        services.AddTransient<InstrumentPanel>();
        services.AddTransient<TextPanelModel>();

        services.AddTransient<GridInstrument>();
        services.AddTransient<ImageInstrument>();
        services.AddTransient<ListInstrument>();
    }

    private void InitializeExtraServices(MacroServiceCollection services)
    {
        if (InitializationException != null) return;

        try
        {
            foreach (var module in Macro.Modules)
            {
                module.OnMacroConfigure(services);
            }

            Macro.ConfigureServices(services);
        }
        catch (Exception e)
        {
            InitializationException = e;
        }
    }

    private void CheckAdmin()
    {
        if (!Macro.RequireAdmin) return;

        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        if (!isAdmin)
        {
            throw new Exception("This macro requires administrator privileges to run.");
        }
    }

    public T GetOption<T>(string key, T def = default) //where T : struct
    {
        if (Options.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        else
        {
            return def;
        }
    }
    public T GetEnvironment<T>(string key, T def = default)
    {
        if (Environments.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        else
        {
            return def;
        }
    }

    public void SetStatistic<T>(string key, T value)
    {
        if (!GetEnvironment<bool>("macro.usestatistics")) return;
        Macro.Statistics.Set(key, value);
        //Log(LogLevel.Debug, $"Set statistic {key} = {value}.");

    }

    public void SetStatistic<T>(string key, Func<T, T> action)
    {
        if (!GetEnvironment<bool>("macro.usestatistics")) return;
        Macro.Statistics.Set(key, action, out var oldValue, out var newValue);
        //Log(LogLevel.Debug, $"Set statistic {key} = {newValue}.");
    }

    public object GetService(Type type)
    {
        return Services.GetService(type);
    }

    public T GetService<T>() where T : class
    {
        return GetService(typeof(T)) as T;
    }

    public void Launch()
    {
        StartTime = DateTime.Now;

        InitializeProcessor();

        GetService<MacroLogger>().UpdateUI();
        Thread.Sleep(100);
        Log(LogLevel.Information, "The macro has started up.");

        Hooks.Raise("ui_ready");

        RaiseEvent(MacroEventType.ProcessStarting, new MacroStartedEventArgs(StartTime));

        foreach (var module in Macro.Modules)
        {
            module.OnMacroProcess(this);
        }
        Macro.Process(this);

        RaiseEvent(MacroEventType.ProcessStarted, new EventArgs());

        Workflow.Start();
    }

    public void Abort()
    {
        Workflow.Abort();
    }

    public void CheckPause()
    {
    }

    protected void Log(LogLevel level, string message, params object[] args)
    {
        GetService<MacroLogger>().Log(level, nameof(MacroProcessor), message, args);
    }

    private void OnEnd(object sender, EndingEventArgs args)
    {
        EndTime = DateTime.Now;

        SetStatistic("LastRunTime", StartTime);
        SetStatistic<int>("TotalRunCount", old => old + 1);
        SetStatistic<TimeSpan>("TotalRunTime", old => old + (EndTime - StartTime));

        var report = new ProcessReport()
        {
            MacroName = Macro.Name,
            ProcessId = ProcessId,
            BeginTime = StartTime,
            EndTime = EndTime,
            EndReason = args.Reason,
        };
        RaiseEvent(MacroEventType.ProcessCompleted, new MacroCompletedEventArgs(args.Reason, report));
    }

    public void RaiseEvent(MacroEventType type, EventArgs eventArgs)
    {
        MulticastDelegate? multicastDelegate = type switch
        {
            MacroEventType.ProcessStarting => Starting,
            MacroEventType.ProcessStarted => Started,
            MacroEventType.ProcessCompleted => Completed,
            MacroEventType.PanelCreated => PanelCreated,
            _ => throw new NotSupportedException(),
        };
        if (multicastDelegate == null) return;

        var handlerArgs = new object[] { this, eventArgs };

        originalContext.Post(d =>
        {
            multicastDelegate?.DynamicInvoke(handlerArgs);
        }, null);
    }

    public void RaiseAction(Action action)
    {
        originalContext.Post(d =>
        {
            action.DynamicInvoke();
        }, null);
    }

    public async Task Pause()
    {
        PauseTokenSource = new();
        PauseToken = PauseTokenSource.Token;
        PauseTokenSource.IsPaused = true;

        await PauseToken.WaitWhilePausedAsync();
        PauseToken = null;
        PauseTokenSource = null;
    }

    public void Resume()
    {
        PauseTokenSource.IsPaused = false;
    }

    public void Dispose()
    {
        Services.Dispose();
    }
}
