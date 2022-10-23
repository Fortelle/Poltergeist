using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities;
using Poltergeist.Services;
using Poltergeist.ViewModels;
using Poltergeist.Views;

namespace Poltergeist;

public abstract class PoltergeistApplication : Application
{
    private static IHost _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var options = CommandLineUtil.GetOptions(e.Args);

        if (SingletonHelper.IsSingleInstance)
        {
            Initialize();

            MainWindow = new MainWindow();
            MainWindow.Show();

            ParseArguments(options);
        }
        else if (Debugger.IsAttached)
        {
            throw new Exception("Another instance is already running.");
        }
        else
        {
            SingletonHelper.SendData(e.Args);
            Shutdown();
        }
    }

    public static void ParseArguments(string[] args)
    {
        var options = CommandLineUtil.GetOptions(args);
        ParseArguments(options);
    }

    private static void ParseArguments(CommandOption[] options)
    {
        foreach(var option in options)
        {
            switch (option.Name)
            {
                case "macro":
                    var immediacy = options.Any(x => x.Name == "immediacy");
                    var mng = App.GetService<MacroManager>();
                    mng.Set(option.Value, immediacy ? LaunchReason.ByCommandLine : null);
                    break;
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        _host?.Dispose();
    }

    public void Initialize()
    {
        // ModernWpf
        var themeResources = new ModernWpf.ThemeResources();
        ((ISupportInitialize)themeResources).BeginInit();
        Resources.MergedDictionaries.Add(themeResources);
        ((ISupportInitialize)themeResources).EndInit();
        Resources.MergedDictionaries.Add(new ModernWpf.Controls.XamlControlsResources());

        // local
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Poltergeist;component/Styles/FontSizes.xaml", UriKind.Absolute) });
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Poltergeist;component/Styles/Thickness.xaml", UriKind.Absolute) });
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Poltergeist;component/Styles/TextBlock.xaml", UriKind.Absolute) });

        _host = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<HomePage>();
                services.AddSingleton<HomeViewModel>();
                services.AddSingleton<ShellPage>();
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<MacroConsolePage>();
                services.AddSingleton<MacroConsoleViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<SchedulerPage>();
                services.AddSingleton<SchedulerViewModel>();
                services.AddSingleton<AboutViewModel>();
                services.AddSingleton<AboutPage>();
                services.AddSingleton<DevelopPage>();

                services.AddSingleton<HotKeyService>();
                services.AddSingleton<LocalSettingsService>();
                services.AddSingleton<NavigationService>();
                services.AddSingleton<PathService>();
                services.AddSingleton<PluginService>();
                services.AddSingleton<SchedulerService>();
                services.AddSingleton<ActionService>();

                services.AddSingleton<MacroManager>();

                services.AddSingleton<ActivationService>();

                services.AddTransient<MacroGroupPage>();
            })
            .Build();

        var activation = GetService<ActivationService>();
        activation.ActivateAsync().ContinueWith(async (t) => {
            var shellpage = GetService<ShellPage>();
            shellpage.Ready();
            await System.Threading.Tasks.Task.CompletedTask;
        });
    }

    public static T GetService<T>() where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }

    public static T GetSettings<T>(string key, T def = default)
    {
        var settingsService = GetService<LocalSettingsService>();
        var value = settingsService.GetSetting<T>(key, def);

        return value;
    }

    public static void SetSettings<T>(string key, T value, bool save = false)
    {
        var settingsService = GetService<LocalSettingsService>();
        settingsService.SetSetting<T>(key, value);
        if (save)
        {
            settingsService.Save();
        }
    }

    public static void ShowFlyout(string message)
    {
        var shell = GetService<ShellPage>();
        shell.ShowFlyout(message);

    }

}
