using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Helpers;
using Poltergeist.Helpers.Converters;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;

namespace Poltergeist.UI.Pages.Macros;

public sealed partial class MacroPage : Page, IPageClosing, IApplicationClosing
{
    public static readonly NavigationInfo NavigationInfo = new()
    {
        Key = "macro",
        CreateContent = (pageKey, data) =>
        {
            var shellKey = pageKey.Split(':')[1];
            var macroManager = App.GetService<MacroManager>();
            var shell = macroManager.GetShell(shellKey);
            if (shell is null)
            {
                throw new Exception($"Invalid shell key '{shellKey}'.");
            }
            if (shell.Template is null)
            {
                throw new Exception(App.Localize($"Poltergeist/Macros/MacroNotExist", shell.TemplateKey));
            }

            var macroPage = new MacroPage(new(shell));
            return macroPage;
        },
        CreateHeader = page =>
        {
            var macroPage = (MacroPage)page;
            var textblock = new TextBlock()
            {
                Text = macroPage.ViewModel?.Shell.Title,
            };
            var icon = new FontIcon()
            {
                Glyph = "\uEDB5",
                Visibility = Visibility.Collapsed,
                DataContext = macroPage.ViewModel,
                FontSize = 12,
            };
            var binding = new Binding()
            {
                Path = new PropertyPath("IsRunning"),
                Mode = BindingMode.OneWay,
                Converter = new FalsyToVisibilityConverter(),
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
        CreateIcon = page =>
        {
            var macroPage = (MacroPage)page;
            if (macroPage.ViewModel.Shell.Icon is string s)
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
        if (ViewModel.Shell.Template!.ConfigVariations.Count == 0)
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
    
    private async void ActionExecuteButton_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not MacroAction action)
        {
            return;
        }

        var macroManager = App.GetService<MacroManager>();

        var macro = ViewModel.Shell.Template;
        var options = macroManager.GetOptions(ViewModel.Shell);
        var environments = macroManager.GetEnvironments(ViewModel.Shell);

        if (macro is null)
        {
            App.ShowTeachingTip("The macro does not exist.");
            return;
        }

        if (macro.Status != MacroStatus.Initialized)
        {
            App.ShowTeachingTip("The macro has not been initialized correctly.");
            return;
        }

        if (!macro.Actions.Contains(action))
        {
            App.ShowTeachingTip("The action is not owned by the macro.");
            return;
        }

        static void OnMessageReceived(string message)
        {
            App.ShowTeachingTip(message);
        }

        var args = new MacroActionArguments((IUserMacro)macro)
        {
            Options = options,
            Environments = environments,
        };
        args.MessageReceived += OnMessageReceived;
        await ExecuteAction(action, args);
        args.MessageReceived -= OnMessageReceived;
    }

    private static async Task ExecuteAction(MacroAction action, MacroActionArguments arguments)
    {
        if (action.Execute is not null)
        {
            try
            {
                action.Execute(arguments);

                if (!string.IsNullOrEmpty(arguments.Message))
                {
                    App.ShowTeachingTip(arguments.Message);
                }
            }
            catch (Exception exception)
            {
                App.ShowException(exception);
            }
        }
        else if (action.ExecuteAsync is not null)
        {
            CancellationTokenSource? cts = null;

            if (action.IsCancellable)
            {
                cts = new();
                arguments.CancellationToken = cts.Token;
            }

            _ = InteractionHelper.Interact(new ProgressModel()
            {
                IsOn = true,
                Title = action.ProgressTitle ?? action.Text,
                CancellationTokenSource = cts,
            });

            try
            {
                await action.ExecuteAsync(arguments);

                if (!string.IsNullOrEmpty(arguments.Message))
                {
                    App.ShowTeachingTip(arguments.Message);
                }
            }
            catch (Exception exception)
            {
                App.ShowException(exception);
            }

            _ = InteractionHelper.Interact(new ProgressModel()
            {
                IsOn = false,
            });

            cts?.Dispose();
        }
        else
        {
            _ = InteractionService.UIShowAsync(new TipModel()
            {
                Text = "The action is empty.",
            });
        }
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

        try
        {
            ViewModel.Shell.UserOptions?.Save();
        }
        catch (Exception exception)
        {
            App.ShowException(exception);
        }

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
