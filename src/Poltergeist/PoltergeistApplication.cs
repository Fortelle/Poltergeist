using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.Hosting;
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

    private static WindowEx? _mainWindow;
    public static WindowEx MainWindow => _mainWindow!;

    public static UIElement? AppTitlebar { get; set; }

    public static bool IsAdministrator { get; private set; }
    public static bool IsDevelopment { get; set; }
    public static string? SingleMacroMode { get; set; }
    public static string? StartPageKey { get; set; }

    public static PoltergeistApplication CurrentPoltergeist => (PoltergeistApplication)Current;

    public IHost? Host { get; private set; }

    public static PathProvider Paths { get; } = new();

    protected static AppLogWrapper? Logger { get; set; }

    protected PoltergeistApplication()
    {
        IsDevelopment = Debugger.IsAttached;
        IsDevelopment |= CommandLineService.StartupOptions.Contains("dev");

        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        IsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);
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

        _mainWindow = new MainWindow();
        _mainWindow.Closed += AppWindow_Closed;
        MainWindow.Show();

        Initialize();

        Task.Run(() =>
        {
            ConfigureAutoLoadServices();

            OnContentLoading();

            GetService<AppEventService>().Raise<AppContentLoadedHandler>();

            TryEnqueue(() =>
            {
                MainWindow.Content = GetService<ShellPage>();

                GetService<AppEventService>().Raise<AppWindowLoadedHandler>();

                Logger?.Debug("Application initialized.");

                Logger?.Trace(new string('-', 64));
            });
        });
    }

    private void Initialize()
    {
        AppSettingsService.Load();

        ConfigureResources(Resources.MergedDictionaries);

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(ConfigureServices)
            .Build();

        Logger = new AppLogWrapper(this);

        UnhandledException += App_UnhandledException;
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
        SingleInstanceHelper.Close();

        App.GetService<AppEventService>().Raise<AppWindowClosedHandler>();

        Host?.Dispose();
    }

    private void OnContentLoading()
    {
        GetService<AppEventService>().SubscribeMethods(this);

        ConfigureInstruments(GetService<InstrumentManager>());
        ConfigureNavigations(GetService<INavigationService>());
        ConfigureSettings(GetService<AppSettingsService>());
        ConfigureHotKeys(GetService<HotKeyService>());
        ConfigureCommandLineParsers(GetService<CommandLineService>());

        InteractionService.Interacting = async (e) =>
        {
            await InteractionHelper.Interact(e.Model);
        };

        GetService<AppEventService>().Raise<AppContentLoadingHandler>();
    }

}
