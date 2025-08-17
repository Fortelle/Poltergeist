using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Helpers;
using Poltergeist.Modules.App;
using Poltergeist.Modules.CommandLine;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.HotKeys;
using Poltergeist.Modules.Instruments;
using Poltergeist.Modules.Logging;
using Poltergeist.Modules.Navigation;
using Poltergeist.Modules.Settings;
using Poltergeist.UI.Windows;

namespace Poltergeist;

public abstract partial class PoltergeistApplication : Application
{
    public const string ApplicationName = "Poltergeist";

    public static new PoltergeistApplication Current => (PoltergeistApplication)Application.Current;

    private WindowEx? _mainWindow;
    public WindowEx MainWindow => _mainWindow!;

    public UIElement? AppTitlebar { get; set; }

    public CommandLineOptionCollection StartupOptions { get; }

    public bool IsAdministrator { get; }
    public bool IsDevelopment { get; }
    public string? ExclusiveMacroMode { get; set; }
    public string? StartPageKey { get; set; }

    public static PathProvider Paths { get; } = new();

    public AppSettings Settings { get; }

    public AppInternalSettings InternalSettings { get; }

    public IHost? Host { get; private set; }

    public ApplicationState State { get; private set; }

    public bool IsReady => State == ApplicationState.Launched;

    public DispatcherQueue DispatcherQueue => MainWindow.DispatcherQueue;

    private AppLogWrapper? Logger;

    protected PoltergeistApplication()
    {
        State = ApplicationState.Launching;

        StartupOptions = new(Environment.GetCommandLineArgs()[1..]);

        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);
        
        IsDevelopment = Debugger.IsAttached;
        IsDevelopment |= StartupOptions.Contains("dev");

        Settings = new(Paths.AppSettingsFile);

        InternalSettings = new(Paths.AppInternalSettingsFile);

        UnhandledException += App_UnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        if (!SingleInstanceHelper.IsFirstInstance())
        {
            CommandLineService.Send(Environment.GetCommandLineArgs()[1..]);

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Exit();

            return;
        }

        Initialize();

        _mainWindow = new MainWindow();
        _mainWindow.Closed += AppWindow_Closed;
        MainWindow.Show();

        GetService<AppEventService>().Subscribe<AppShellPageLoadedEvent>(_ =>
        {
            GetService<AppEventService>().Publish<AppWindowLoadedEvent>();

            Logger?.Debug("Application initialized.");

            Logger?.Trace(new string('-', 64));

            State = ApplicationState.Launched;

            GetService<AppEventService>().Publish<AppLaunchedEvent>();
        });

        Task.Run(() =>
        {
            GetService<AppEventService>().Publish<AppWindowActivatedEvent>();

            ConfigureAutoLoadServices();

            OnContentLoading();

            GetService<AppEventService>().Publish<AppContentLoadingEvent>();

            TryEnqueue(() =>
            {
                var shellpage = GetService<ShellPage>();
                MainWindow.Content = shellpage; // emits AppShellPageLoadedEvent on loaded
            });
        });
    }

    private void Initialize()
    {
        ConfigureResources(Resources.MergedDictionaries);

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(ConfigureServices)
            .Build();

        Logger = new AppLogWrapper(this);

        GetService<AppEventService>().Publish<AppInitializedEvent>();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        Logger?.Critical($"Unhandled exception: {e.Message}", new
        {
            Type = e.Exception.GetType().Name,
            Message = e.Exception.Message,
        });
    }

    private void AppWindow_Closed(object sender, WindowEventArgs args)
    {
        State = ApplicationState.Exiting;

        App.GetService<AppEventService>().Publish<AppWindowClosedEvent>();

        App.GetService<AppEventService>().Publish<AppExitingEvent>();

        Host?.Dispose();
        
        SingleInstanceHelper.Close();

        State = ApplicationState.Exited;
    }

    private void OnContentLoading()
    {
        var eventService = GetService<AppEventService>();
        eventService.SubscribeMethods(this);

        ConfigureInstruments(GetService<InstrumentManager>());
        ConfigureNavigations(GetService<INavigationService>());
        ConfigureSettings(GetService<AppSettingsService>().Settings);
        ConfigureHotKeys(GetService<HotKeyService>());
        ConfigureCommandLineParsers(GetService<CommandLineService>());

        InteractionService.Interacting = async (e) =>
        {
            await InteractionHelper.Interact(e.Model);
        };
    }

}
