using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Macros;
using Poltergeist.Helpers;
using Poltergeist.Helpers.Converters;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;

namespace Poltergeist.UI.Pages.Macros;

public sealed partial class TerminalActionPage : Page, IPageClosing
{
    public static readonly PageInfo PageInfo = new("quest")
    {
        CreateContent = (pageKey, data) =>
        {
            MacroInstance? instance;
            var parts = pageKey.Split(':');

            if (data is MacroInstance mi)
            {
                instance = mi;
            }
            else
            {
                var instanceId = parts[1];
                instance = App.GetService<MacroInstanceManager>().GetInstance(instanceId)!;
            }
            var actionIndex = int.Parse(parts[2]);
            var action = (TerminalAction)instance.Template!.Actions[actionIndex];
            var macroPage = new TerminalActionPage(new(action, instance));
            return macroPage;
        },
        CreateHeader = page =>
        {
            var macroPage = (TerminalActionPage)page;
            var textblock = new TextBlock()
            {
                Text = macroPage.ViewModel?.Title,
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
                Converter = new FalsyToCollapsedConverter(),
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
            var macroPage = (TerminalActionPage)page;
            return IconInfoHelper.ConvertToIconSource(macroPage.ViewModel?.Instance.Icon);
        },
    };

    public TerminalActionViewModel? ViewModel { get; set; }

    public TerminalActionPage(TerminalActionViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        ViewModel!.RichTextBlock = TextPanelBox;

        if (ViewModel.Quest.AutoExecute)
        {
            _ = ViewModel.Start();
        }
    }

    bool IPageClosing.OnPageClosing()
    {
        if (ViewModel is null)
        {
            return true;
        }

        if (ViewModel.IsRunning)
        {
            return false;
        }

        return true;
    }
}
