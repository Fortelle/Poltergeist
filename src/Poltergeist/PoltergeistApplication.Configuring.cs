using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;
using Poltergeist.Helpers;
using Poltergeist.Modules.App;
using Poltergeist.Modules.CommandLine;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.HotKeys;
using Poltergeist.Modules.Instruments;
using Poltergeist.Modules.Interactions;
using Poltergeist.Modules.Logging;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;
using Poltergeist.Modules.Pipes;
using Poltergeist.Modules.Settings;
using Poltergeist.UI.Controls.Instruments;
using Poltergeist.UI.Pages.About;
using Poltergeist.UI.Pages.Home;
using Poltergeist.UI.Pages.Logging;
using Poltergeist.UI.Pages.Macros;
using Poltergeist.UI.Pages.Settings;
using Poltergeist.UI.Windows;

namespace Poltergeist;

public partial class PoltergeistApplication
{

    protected virtual void ConfigureResources(IList<ResourceDictionary> dictionaries)
    {
        dictionaries.Add(new XamlControlsResources());
        dictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/FontSizes.xaml") });
        dictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/Layouts.xaml") });
        dictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/TextBlock.xaml") });
        dictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Styles/Thickness.xaml") });
        dictionaries.Add(new ResourceDictionary() { Source = new Uri(@"ms-appx:///Poltergeist/Helpers/Converters/Converters.xaml") });
    }

    protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Services
        services.AddSingleton<AppSettingsService>();
        services.AddSingleton<PipeService>();
        services.AddSingleton<CommandLineService>();
        services.AddSingleton<AppLoggingService>();

        services.AddSingleton<INavigationService, NavigationService>();

        // Views and ViewModels
        services.AddSingleton<ShellPage>();
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<AboutPage>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MainPage>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoggingPage>();
        services.AddTransient<LoggingViewModel>();

        services.AddSingleton<PluginService>();

        services.AddSingleton<ActionService>();
        services.AddSingleton<MacroManager>();
        services.AddSingleton<HotKeyService>();
        services.AddSingleton<InstrumentManager>();
        services.AddSingleton<TipService>();
        services.AddSingleton<DialogService>();
        services.AddSingleton<AppNotificationService>();

        services.AddSingleton<AppEventService>();
    }

    protected virtual void ConfigureCommandLineParsers(CommandLineService commandLineService)
    {
        commandLineService.AddParser<MacroCommandLineParser>();
    }

    protected virtual void ConfigureAutoLoadServices()
    {
        GetService<AppSettingsService>();
        GetService<PluginService>();
        GetService<MacroManager>();
        GetService<PipeService>();

        RestorePreviousTabsHelper.Inject();
    }

    protected virtual void ConfigureInstruments(InstrumentManager instrumentManager)
    {
        instrumentManager.AddInfo<TextInstrument, TextInstrumentView, TextInstrumentViewModel>();
        instrumentManager.AddInfo<IListInstrumentModel, ListInstrumentView, ListInstrumentViewModel>();
        instrumentManager.AddInfo<IGridInstrumentModel, GridInstrumentView, GridInstrumentViewModel>();
        instrumentManager.AddInfo<ImageInstrument, ImageInstrumentView, ImageInstrumentViewModel>();
    }

    protected virtual void ConfigureNavigations(INavigationService navigationService)
    {
        StartPageKey ??= "home";

        navigationService.AddInfo(new()
        {
            Key = "home",
            Header = Localize($"Poltergeist/Resources/TabHeader_Home"),
            Icon = new("\uE80F"),
            CreateContent = (_, _) => App.GetService<MainPage>(),
            PositionInSidebar = NavigationItemPosition.Top,
        });

        navigationService.AddInfo(new()
        {
            Key = "about",
            Header = Localize($"Poltergeist/Resources/TabHeader_About"),
            Icon = new("\uE9CE"),
            CreateContent = (_, _) => App.GetService<AboutPage>(),
            PositionInSidebar = NavigationItemPosition.Bottom,
        });
        navigationService.AddInfo(new()
        {
            Key = "settings",
            Header = Localize($"Poltergeist/Resources/TabHeader_Settings"),
            Icon = new("\uE713"),
            CreateContent = (_, _) => App.GetService<SettingsPage>(),
            PositionInSidebar = NavigationItemPosition.Bottom,
        });
        navigationService.AddInfo(new()
        {
            Key = "log",
            Header = Localize($"Poltergeist/Resources/TabHeader_Log"),
            Icon = new("\uF0E3"),
            CreateContent = (_, _) => App.GetService<LoggingPage>(),
            PositionInSidebar = NavigationItemPosition.Bottom,
        });
#if DEBUG
        navigationService.AddInfo(new()
        {
            Key = "debug",
            Header = "Debug",
            Icon = new("\uEBE8"),
            Action = DebugHelper.Do,
            PositionInSidebar = NavigationItemPosition.Bottom,
        });
#endif
        navigationService.AddInfo(MacroPage.NavigationInfo);
    }

    protected virtual void ConfigureSettings(AppSettingsService settingsService)
    {
        settingsService.Add(new OptionDefinition<int>(MacroBrowserViewModel.LastSortIndexKey)
        {
            Status = ParameterStatus.Hidden,
        });

        // Macro

        settingsService.Add(new OptionDefinition<LogLevel>(MacroLogger.FileLogLevelKey, LogLevel.None)
        {
            Category = App.Localize($"Poltergeist/Resources/AppSettings_Macro"),
            DisplayLabel = App.Localize($"Poltergeist/Resources/AppSettings_Macro_LogToFile"),
        });

        settingsService.Add(new OptionDefinition<LogLevel>(MacroLogger.FrontLogLevelKey, LogLevel.Information)
        {
            Category = App.Localize($"Poltergeist/Resources/AppSettings_Macro"),
            DisplayLabel = App.Localize($"Poltergeist/Resources/AppSettings_Macro_LogToConsole"),
        });
    }

    protected virtual void ConfigureHotKeys(HotKeyService hotkeyService)
    {
        hotkeyService.Add(new("macrostartkey")
        {
            SettingDefinition = new("macro.startkey")
            {
                Category = ResourceHelper.Localize("Poltergeist/Resources/AppSettings_Macro"),
                DisplayLabel = ResourceHelper.Localize("Poltergeist/Resources/AppSettings_Macro_StartMacroHotKey"),
            },

            Callback = () =>
            {
                TryEnqueue(() =>
                {
                    ActionService.RunMacro(LaunchReason.ByUser, true);
                });
            },
        });
    }

}
