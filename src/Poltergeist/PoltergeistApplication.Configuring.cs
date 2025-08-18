using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures.Parameters;
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
        // Services<>
        services.AddSingleton<AppSettingsService>();
        services.AddSingleton<PipeService>();
        services.AddSingleton<CommandLineService>();
        services.AddSingleton<AppLoggingService>();
        services.AddSingleton<AppEventService>();

        services.AddSingleton<NavigationService>();

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

        services.AddSingleton<ActionService>();
        services.AddSingleton<MacroManager>();
        services.AddSingleton<MacroInstanceManager>();
        services.AddSingleton<MacroTemplateManager>();
        services.AddSingleton<HotKeyService>();
        services.AddSingleton<InstrumentManager>();
        services.AddSingleton<TipService>();
        services.AddSingleton<DialogService>();
        services.AddSingleton<AppNotificationService>();
        services.AddSingleton<GlobalOptionsService>();
        services.AddSingleton<MacroStatisticsService>();

        services.AddSingleton<TextToSpeechService>();
        services.AddSingleton<AudioPlayerService>();
        services.AddSingleton<SoundEffectService>();
    }

    protected virtual void OnContentLoading()
    {
        GetService<AppEventService>().SubscribeMethods(this);

        ConfigureServices();

        ConfigureSettings();
        ConfigureNavigations();
        ConfigureHotKeys();
        ConfigureInstruments();
        ConfigureCommandLineParsers();

        InteractionService.Interacting = async (e) =>
        {
            await InteractionHelper.Interact(e.Model);
        };

        RestorePreviousTabsHelper.Inject();

        GetService<AppEventService>().Publish<AppContentLoadingEvent>();
    }

    private static void ConfigureServices()
    {
        GetService<MacroManager>();
        GetService<PipeService>();
        //GetService<TextToSpeechService>(); // todo: not now
        GetService<AppSettingsService>();
    }

    private static void ConfigureInstruments()
    {
        var instrumentManager = GetService<InstrumentManager>();
        instrumentManager.AddInfo<TextInstrument, TextInstrumentView, TextInstrumentViewModel>();
        instrumentManager.AddInfo<IListInstrumentModel, ListInstrumentView, ListInstrumentViewModel>();
        instrumentManager.AddInfo<ITileInstrumentModel, TileInstrumentView, TileInstrumentViewModel>();
        instrumentManager.AddInfo<ImageInstrument, ImageInstrumentView, ImageInstrumentViewModel>();
    }

    private static void ConfigureSettings()
    {
        var settingsService = GetService<AppSettingsService>();
        settingsService.Settings.AddDefinition(new OptionDefinition<bool>("app_show_performance", false)
        {
            Status = ParameterStatus.DevelopmentOnly,
            Category = PoltergeistApplication.Localize($"Poltergeist/Resources/AppSettings_App"),
            DisplayLabel = "Show performance",
        });
    }

    private static void ConfigureNavigations()
    {
        var navigationService = GetService<NavigationService>();

        navigationService.AddSidebarItemInfo(new SidebarItemInfo()
        {
            Text = Localize($"Poltergeist/Resources/TabHeader_Home"),
            Icon = new("\uE80F"),
            Position = SidebarItemPosition.Top,
            Navigation = new("home"),
        });
        navigationService.AddPageInfo(new PageInfo("home")
        {
            Header = Localize($"Poltergeist/Resources/TabHeader_Home"),
            Icon = new("\uE80F"),
            CreateContent = (_, _) => App.GetService<MainPage>(),
        });

        navigationService.AddSidebarItemInfo(new SidebarItemInfo()
        {
            Text = Localize($"Poltergeist/Resources/TabHeader_About"),
            Icon = new("\uE9CE"),
            Position = SidebarItemPosition.Bottom,
            Navigation = new("about"),
        });
        navigationService.AddPageInfo(new PageInfo("about")
        {
            Header = Localize($"Poltergeist/Resources/TabHeader_About"),
            Icon = new("\uE9CE"),
            CreateContent = (_, _) => App.GetService<AboutPage>(),
        });

        navigationService.AddSidebarItemInfo(new SidebarItemInfo()
        {
            Text = Localize($"Poltergeist/Resources/TabHeader_Settings"),
            Icon = new("\uE713"),
            Position = SidebarItemPosition.Bottom,
            Navigation = new("settings"),
        });
        navigationService.AddPageInfo(new PageInfo("settings")
        {
            Header = Localize($"Poltergeist/Resources/TabHeader_Settings"),
            Icon = new("\uE713"),
            CreateContent = (_, _) => App.GetService<SettingsPage>(),
        });

        navigationService.AddSidebarItemInfo(new SidebarItemInfo()
        {
            Text = Localize($"Poltergeist/Resources/TabHeader_Log"),
            Icon = new("\uF0E3"),
            Navigation = new("log"),
            Position = SidebarItemPosition.Bottom,
        });
        navigationService.AddPageInfo(new PageInfo("log")
        {
            Header = Localize($"Poltergeist/Resources/TabHeader_Log"),
            Icon = new("\uF0E3"),
            CreateContent = (_, _) => App.GetService<LoggingPage>(),
        });

#if DEBUG
        navigationService.AddSidebarItemInfo(new SidebarItemInfo()
        {
            Text = "Debug",
            Icon = new("\uEBE8"),
            Action = DebugHelper.Do,
            Position = SidebarItemPosition.Bottom,
        });
#endif

        navigationService.AddPageInfo(MacroPage.PageInfo);
    }

    private static void ConfigureCommandLineParsers()
    {
        var commandLineService = GetService<CommandLineService>();
        commandLineService.AddParser<MacroCommandLineParser>();
    }

    private static void ConfigureHotKeys()
    {
        var hotkeyService = GetService<HotKeyService>();
        hotkeyService.Add(MacroStartKeyHelper.HotKeyInformation);

        var settingsService = GetService<AppSettingsService>();
        settingsService.WatchChange<Automations.Utilities.Windows.HotKey>(MacroStartKeyHelper.HotKeyInformation.SettingDefinition!.Key, MacroStartKeyHelper.OnSettingChanged);
    }

}
