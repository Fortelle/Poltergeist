using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Helpers.Converters;
using Poltergeist.Services;

namespace Poltergeist.Pages.Macros;

public sealed partial class MacroPage : Page, IPageClosing, IApplicationClosing
{
    public static readonly NavigationInfo NavigationInfo = new()
    {
        Key = "macro",
        CreateContent = (args, obj) =>
        {
            if (args.Length == 0)
            {
                return null;
            }

            var shellKey = args[0];
            var macroManager = App.GetService<MacroManager>();
            var shell = macroManager.GetShell(shellKey);
            if (shell is null)
            {
                App.ShowTeachingTip(App.Localize($"Poltergeist/Macros/MacroNotExist", shellKey));
                return null;
            }
            if (shell.Template is null)
            {
                App.ShowTeachingTip(App.Localize($"Poltergeist/Macros/MacroNotExist", shell.TemplateKey));
                return null;
            }

            var macroPage = new MacroPage(new(shell));
            return macroPage;
        },
        CreateHeader = obj =>
        {
            var page = (MacroPage)obj;
            var textblock = new TextBlock()
            {
                Text = page.ViewModel?.Shell.Title,
            };
            var icon = new FontIcon()
            {
                Glyph = "\uEDB5",
                Visibility = Visibility.Collapsed,
                DataContext = page.ViewModel,
                FontSize = 12,
            };
            var binding = new Binding()
            {
                Path = new PropertyPath("IsRunning"),
                Mode = BindingMode.OneWay,
                Converter = new BoolToVisibilityConverter(),
            };
            icon.SetBinding(FontIcon.VisibilityProperty, binding);

            Grid.SetColumn(textblock, 0);
            Grid.SetColumn(icon, 1);

            var grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ColumnDefinitions =
                {
                    new() { Width = new GridLength(1, GridUnitType.Star) },
                    new() { Width = new GridLength(0, GridUnitType.Auto) },
                },
                RowDefinitions =
                {
                    new()
                },
                Children =
                {
                    textblock,
                    icon,
                },
            };
            return grid;
        },
        CreateIcon = obj =>
        {
            var page = (MacroPage)obj;
            if (page.ViewModel.Shell.Icon is string s)
            {
                var icon = new IconInfo(s);
                var source = icon.ToIconSource();
                if (source is not null)
                {
                    return source;
                }
            }

            return new IconInfo(MacroShell.DefaultIconUri).ToIconSource();
        },
    };

    public MacroViewModel ViewModel { get; }

    public MacroPage(MacroViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        LoadConfigVariations();
    }

    private void LoadConfigVariations()
    {
        if (!ViewModel.Shell.Template!.ConfigVariations.Any())
        {
            return;
        }

        RunMenuFlyout.Items.Add(new MenuFlyoutSeparator());

        for (var i = 0; i < ViewModel.Shell.Template.ConfigVariations.Count; i++)
        {
            var variation = ViewModel.Shell.Template.ConfigVariations[i];

            if (variation.IsDevelopmentOnly && !App.IsDevelopment)
            {
                continue;
            }

            var mfi = new MenuFlyoutItem()
            {
                Text = variation.Title ?? App.Localize($"Poltergeist/Macros/Run_Variation", i + 1),
                Command = ViewModel.StartCommand,
                CommandParameter = new MacroStartArguments()
                {
                    ShellKey = ViewModel.Shell.ShellKey,
                    Reason = LaunchReason.ByUser,
                    IgnoresUserOptions = variation.IgnoresUserOptions,
                    OptionOverrides = variation.OptionOverrides,
                    EnvironmentOverrides = variation.EnvironmentOverrides,
                    SessionStorage = variation.SessionStorage,
                },
                Icon = new IconInfo(variation.Icon ?? "\uE768").ToIconElement(),
        };
            RunMenuFlyout.Items.Add(mfi);
        }
    }
    
    private void ActionExecuteButton_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroAction action)
        {
            return;
        }

        var macroManager = App.GetService<MacroManager>();

        var macro = (MacroBase)ViewModel.Shell.Template!;
        var options = macroManager.GetOptions(ViewModel.Shell);
        var environments = macroManager.GetEnvironments(ViewModel.Shell);

        var args = new MacroActionArguments(macro)
        {
            Options = options,
            Environments = environments,
        };
        ViewModel.Shell.ExecuteAction(action, args);
    }

    public bool OnPageClosing()
    {
        if (ViewModel is null)
        {
            return true;
        }

        if (ViewModel.IsRunning)
        {
            return false;
        }

        ViewModel.Shell.UserOptions?.Save();

        return true;
    }

    public bool OnApplicationClosing()
    {
        if (ViewModel is null)
        {
            return true;
        }

        ViewModel.Shell.UserOptions?.Save();

        return true;
    }

    private void HistoryListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ProcessHistoryEntry historyEntry)
        {
            if(ViewModel.Shell.PrivateFolder is null)
            {
                return;
            }

            var logFile = Path.Combine(ViewModel.Shell.PrivateFolder, "Logs", historyEntry.ProcessId + ".log");
            if(!File.Exists(logFile))
            {
                App.ShowTeachingTip(App.Localize($"Poltergeist/Macros/LogNotExist"));
                return;
            }

            var process = new Process
            {
                StartInfo = new (logFile)
                {
                    UseShellExecute = true
                }
            };
            process.Start();
        }
    }

}
