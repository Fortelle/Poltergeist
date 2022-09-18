using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernWpf.Controls;
using Poltergeist.Models;
using Poltergeist.Services;
using Poltergeist.ViewModels;
using Poltergeist.Views;

namespace Poltergeist;

public abstract class PoltergeistApplication : Application
{
    private static IHost _host;
    private readonly Mutex _mutex = new Mutex(true, "Poltergeist");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (_mutex.WaitOne(TimeSpan.Zero))
        {
            Initialize();

            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        else
        {
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        _host.Dispose();
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
        var value = settingsService.ReadSetting<T>(key, def);

        return value;
    }

    public static void SetSettings<T>(string key, T value)
    {
        var settingsService = GetService<LocalSettingsService>();
        settingsService.SaveSetting<T>(key, value);
    }

    public static void ShowFlyout(string message)
    {
        var flyout = new Flyout
        {
            Content = new TextBlock()
            {
                Text = message
            },
            ShowMode = ModernWpf.Controls.Primitives.FlyoutShowMode.Transient,
            Placement = ModernWpf.Controls.Primitives.FlyoutPlacementMode.Top,
        };

        flyout.ShowAt(Application.Current.MainWindow.Content as FrameworkElement);
    }
}
