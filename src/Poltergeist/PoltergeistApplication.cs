﻿using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Activation;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities;
using Poltergeist.Contracts.Services;
using Poltergeist.Helpers;
using Poltergeist.Macros.Instruments;
using Poltergeist.Models;
using Poltergeist.Notifications;
using Poltergeist.Pages.About;
using Poltergeist.Pages.Groups;
using Poltergeist.Pages.Macros;
using Poltergeist.Pages.Macros.Instruments;
using Poltergeist.Services;
using Poltergeist.ViewModels;
using Poltergeist.Views;

namespace Poltergeist;

public abstract class PoltergeistApplication : Application
{
    private static WindowEx? _mainWindow;
    public static WindowEx MainWindow => _mainWindow!;

    public static UIElement? AppTitlebar { get; set; }

    public IHost? Host { get; private set; }

    public static event Action? Initialized;
    public static event Action<LocalSettingsService>? SettingsLoading;
    public static event Action<LocalSettingsService>? SettingsLoaded;
    public static event Action? ContentLoading;
    public static event Action? ContentLoaded;

    public static bool IsDevelopment { get; set; }
    public static bool IsLoaded { get; set; }
    public static string? SingleMacroMode { get; set; }

    public static PoltergeistApplication CurrentPoltergeist => (PoltergeistApplication)Current;

    public void Initialize()
    {
        IsDevelopment = Debugger.IsAttached;

        Resources.MergedDictionaries.Add(new XamlControlsResources());
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/FontSizes.xaml") });
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/Layouts.xaml") });
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/TextBlock.xaml") });
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/Thickness.xaml") });
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Helpers/Converters/Converters.xaml") });

        Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers
                //services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

                // Services
                services.AddSingleton<LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();

                services.AddSingleton<INavigationService, NavigationService>();

                // Views and ViewModels
                services.AddSingleton<ShellPage>();
                services.AddSingleton<ShellViewModel>();
                services.AddTransient<AboutPage>();
                services.AddTransient<AboutViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainPage>();

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));

                services.AddSingleton<PathService>();
                services.AddSingleton<PluginService>();

                services.AddSingleton<ActionService>();
                services.AddSingleton<MacroManager>();
                services.AddSingleton<HotKeyService>();
                services.AddSingleton<InstrumentManager>();
                services.AddSingleton<TipService>();
                services.AddSingleton<DialogService>();
                services.AddSingleton<AppNotificationService>();

                services.AddSingleton<HotKeyManager>();

                ConfigureServices(context, services);
            }).
            Build();

        UnhandledException += App_UnhandledException;
    }

    public virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {

    }

    public static T GetService<T>() where T : class
    {
        var service = GetService(typeof(T));
        return (T)service!;
    }

    public static object GetService(Type type)
    {
        var service = CurrentPoltergeist.Host?.Services.GetService(type);
        if (service is null)
        {
            throw new ArgumentException($"{type} needs to be registered in ConfigureServices within App.xaml.cs.");
        }
        return service;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    public virtual void ParseArguments(CommandOption[] options)
    {
        foreach (var (name, value) in options)
        {
            switch (name)
            {
                case "macro" when !string.IsNullOrEmpty(value):
                    {
                        var nav = App.GetService<INavigationService>();
                        if (!nav.NavigateTo("macro:" + value))
                        {
                            continue;
                        }

                        var autostart = options.Any(x => x.Name == "autostart");
                        var autoclose = options.Any(x => x.Name == "autoclose");

                        if (autostart)
                        {
                            var args = new MacroStartArguments()
                            {
                                ShellKey = value,
                                Reason = LaunchReason.ByCommandLine,
                            };
                            if (autoclose)
                            {
                                args.OptionOverrides = new()
                                {
                                    ["aftercompletion.action"] = CompletionAction.ExitApplication,
                                };
                            }
                            ActionService.RunMacro(args);
                        }
                    }
                    break;
                case "dev":
                    {
                        IsDevelopment = true;
                    }
                    break;
            }
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var arguments = Environment.GetCommandLineArgs();
        var options = CommandLineUtil.GetOptions(arguments[1..]);

        if (SingletonHelper.IsSingleInstance)
        {
            _mainWindow = new MainWindow();
            MainWindow.Show();

            Initialize();

            Task.Run(() =>
            {
                OnInitialized();

                var localsettingsService = App.GetService<LocalSettingsService>();
                OnSettingsLoading(localsettingsService);
                SettingsLoading?.Invoke(localsettingsService);
                localsettingsService.Load();
                OnSettingsLoaded(localsettingsService);
                SettingsLoaded?.Invoke(localsettingsService);

                OnContentLoading(options); // macros are loaded here
                ContentLoading?.Invoke();

                CheckSingleMacroMode(options);

                MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    var shellPage = App.GetService<ShellPage>();
                    if (SingleMacroMode is null)
                    {
                        GetService<INavigationService>().NavigateTo("home");
                    }
                    else
                    {
                        GetService<INavigationService>().NavigateTo("macro:" + SingleMacroMode);
                    }

                    Task.Run(() =>
                    {
                        Thread.Sleep(100);

                        MainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            MainWindow.Content = shellPage;

                            OnContentLoaded(options);
                            ContentLoaded?.Invoke();

                            ParseArguments(options);

                            SingletonHelper.Load();
                            SingletonHelper.ArgumentReceived += OnArgumentReceived;

                            IsLoaded = true;
                        });
                    });
                });

            });
        }
        else if (App.IsDevelopment)
        {
            throw new Exception("Another instance is already running.");
        }
        else
        {
            SingletonHelper.SendArggument(string.Join("|", arguments[1..]));
            Exit();
            return;
        }

    }

    private void OnArgumentReceived(string argument)
    {
        var arguments = argument.Split("|");
        var options = CommandLineUtil.GetOptions(arguments);
        ActionService.BringToFront();
        MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            ParseArguments(options);
        });
    }

    private static bool CheckSingleMacroMode(CommandOption[] options)
    {
        var singletonOption = options.FirstOrDefault(x => x.Name == "singlemode");
        if (singletonOption is null)
        {
            return false;
        }

        var macroOption = options.FirstOrDefault(x => x.Name == "macro");
        if (macroOption is null || string.IsNullOrEmpty(macroOption.Value))
        {
            return false;
        }

        var macroManager = App.GetService<MacroManager>();
        var macro = macroManager.GetShell(macroOption.Value);
        if (macro is null)
        {
            return false;
        }

        SingleMacroMode = macro.ShellKey;
        return true;
    }

    protected static void OnInitialized()
    {
        App.GetService<LocalSettingsService>();
        App.GetService<HotKeyManager>();
        App.GetService<PluginService>().Load();
        App.GetService<MacroManager>().LoadProperties();

        Initialized?.Invoke();
    }

    protected virtual void OnSettingsLoading(LocalSettingsService localsettingsService)
    {
        localsettingsService.Add(new OptionDefinition<int>(MacroBrowserViewModel.LastSortIndexKey)
        {
            Status = ParameterStatus.Hidden,
        });

        // Macro
        localsettingsService.Add(new OptionDefinition<bool>(MacroBase.UseStatisticsKey, true)
        {
            Category = App.Localize($"Poltergeist/Resources/LocalSettings_Macro"),
            DisplayLabel = App.Localize($"Poltergeist/Resources/LocalSettings_Macro_UseStatistics"),
        });

        localsettingsService.Add(new OptionDefinition<LogLevel>(MacroLogger.FileLogLevelKey, LogLevel.All)
        {
            Category = App.Localize($"Poltergeist/Resources/LocalSettings_Macro"),
            DisplayLabel = App.Localize($"Poltergeist/Resources/LocalSettings_Macro_LogToFile"),
        });

        localsettingsService.Add(new OptionDefinition<LogLevel>(MacroLogger.FrontLogLevelKey, LogLevel.Information)
        {
            Category = App.Localize($"Poltergeist/Resources/LocalSettings_Macro"),
            DisplayLabel = App.Localize($"Poltergeist/Resources/LocalSettings_Macro_LogToConsole"),
        });
    }

    protected virtual void OnSettingsLoaded(LocalSettingsService localsettingsService)
    {
    }

    protected virtual void OnContentLoading(CommandOption[] options)
    {
        var instrumentService = App.GetService<InstrumentManager>();
        instrumentService.AddInfo<TextInstrument, TextInstrumentView, TextInstrumentViewModel>();
        instrumentService.AddInfo<IListInstrumentModel, ListInstrumentView, ListInstrumentViewModel>();
        instrumentService.AddInfo<IGridInstrumentModel, GridInstrumentView, GridInstrumentViewModel>();
        instrumentService.AddInfo<ImageInstrument, ImageInstrumentView, ImageInstrumentViewModel>();

        var navigationService = App.GetService<INavigationService>();
        navigationService.AddInfo(new()
        {
            Key = "home",
            Icon = new("\uE80F"),
            CreateContent = (_, _) => App.GetService<MainPage>(),
        });
        navigationService.AddInfo(new()
        {
            Key = "settings",
            Icon = new("\uE713"),
            CreateContent = (_, _) => new SettingsPage(),
        });
        navigationService.AddInfo(new()
        {
            Key = "about",
            Icon = new("\uE9CE"),
            CreateContent = (_, _) => App.GetService<AboutPage>(),
        });
        navigationService.AddInfo(new()
        {
            Key = "debug",
            Icon = new("\uEBE8"),
        });

        navigationService.AddInfo(MacroPage.NavigationInfo);

        InteractionService.Interacting += OnInteracting;
    }

    protected virtual void OnContentLoaded(CommandOption[] options)
    {
    }

    public static void ShowTeachingTip(string message)
    {
        TipService.Show(message);
    }

    private async Task OnInteracting(InteractingEventArgs e)
    {
        await Interact(e.Model);
    }

    public static async Task Interact(InteractionModel model)
    {
        switch (model)
        {
            case TipModel tipModel:
                TipService.Show(tipModel);
                break;
            case DialogModel dialogModel:
                await DialogService.ShowAsync(dialogModel);
                break;
            case ToastModel toastModel:
                App.GetService<AppNotificationService>().Show(toastModel);
                break;
            case FileOpenModel fileOpenModel:
                await DialogService.ShowFileOpenPickerAsync(fileOpenModel);
                break;
            case FileSaveModel fileSaveModel:
                await DialogService.ShowFileSavePickerAsync(fileSaveModel);
                break;
            case FolderModel folderModel:
                await DialogService.ShowFolderPickerAsync(folderModel);
                break;
            case NavigationModel navigationModel:
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    App.GetService<INavigationService>().NavigateTo(navigationModel.PageKey, navigationModel.Argumment);
                });
                break;
            case ProgressModel progressModel:
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    DialogService.ShowProgress(progressModel);
                });
                break;
            case AppWindowModel appWindowModel:
                switch (appWindowModel.Action)
                {
                    case AppWindowAction.Minimize:
                        ActionService.MinimizeApplication();
                        break;
                    case AppWindowAction.Restore:
                        ActionService.RestoreApplication();
                        break;
                }
                break;
        }
    }

    public static string Localize(string resourceKey, params object?[] args)
    {
        return ResourceHelper.Localize(resourceKey, args);
    }
}
