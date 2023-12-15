using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;
using Poltergeist.Helpers.Converters;
using Poltergeist.Services;

namespace Poltergeist.Pages.Macros;

public sealed partial class MacroPage : Page, IPageClosing, IApplicationClosing
{
    public static readonly NavigationInfo NavigationInfo = new()
    {
        Key = "macro",
        Glyph = "\uF259",
        CreateContent = (args, obj) =>
        {
            if (args.Length == 0)
            {
                return null;
            }

            var macrokey = args[0];
            var macroManager = App.GetService<MacroManager>();
            var macro = macroManager.GetMacro(macrokey);
            if (macro is null)
            {
                App.ShowTeachingTip(App.Localize($"Poltergeist/Macros/MacroNotExist", macrokey));
                return null;
            }
            if (!macro.IsAvailable)
            {
                App.ShowTeachingTip(App.Localize($"Poltergeist/Macros/MacroUnavailable", macrokey));
                return null;
            }
            
            var macroPage = new MacroPage(new(macro));
            return macroPage;
        },
        CreateHeader = obj =>
        {
            var page = (MacroPage)obj;
            var textblock = new TextBlock()
            {
                Text = page.ViewModel?.Macro.Title,
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
    };

    public MacroViewModel ViewModel { get; }

    public MacroPage(MacroViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        if(((FrameworkElement)sender).DataContext is MacroAction action)
        {
            var macroManager = App.GetService<MacroManager>();

            var options = ViewModel.Macro.GetOptionCollection();
            macroManager.PushGlobalOptions(options);

            var environments = new VariableCollection();
            macroManager.PushEnvironments(environments);

            ViewModel.Macro.ExecuteAction(action, options.ToValueDictionary(), environments.ToValueDictionary());
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

        ViewModel.Macro.UserOptions.Save();

        return true;
    }

    public bool OnApplicationClosing()
    {
        if (ViewModel is null)
        {
            return true;
        }

        ViewModel.Macro.UserOptions.Save();

        return true;
    }

    private void HistoryListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ProcessHistoryEntry historyEntry)
        {
            if(ViewModel.Macro.PrivateFolder is null)
            {
                return;
            }

            var logFile = Path.Combine(ViewModel.Macro.PrivateFolder, "Logs", historyEntry.ProcessId + ".log");
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

internal class EndReasonToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return App.Localize($"Poltergeist/Macros/EndReason_{value}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}