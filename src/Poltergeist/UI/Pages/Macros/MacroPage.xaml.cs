using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Structures;
using Poltergeist.Helpers;
using Poltergeist.Helpers.Converters;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;

namespace Poltergeist.UI.Pages.Macros;

public sealed partial class MacroPage : Page, IPageClosing, IPageClosed
{
    public static readonly PageInfo PageInfo = new("macro")
    {
        CreateContent = (pageKey, data) =>
        {
            MacroInstance? instance;

            if (data is MacroInstance _instance)
            {
                instance = _instance;
            }
            else
            {
                var instanceId = pageKey.Split(':')[1];

                instance = App.GetService<MacroInstanceManager>().GetInstance(instanceId);
                if (instance is null)
                {
                    throw new Exception($"Invalid macro instance id '{instanceId}'.");
                }
                if (instance.Template is null)
                {
                    throw new Exception(App.Localize($"Poltergeist/Macros/MacroNotExist", instance.TemplateKey));
                }
            }

            var macroPage = new MacroPage(new(instance));
            return macroPage;
        },
        CreateHeader = page =>
        {
            var macroPage = (MacroPage)page;
            var textblock = new TextBlock()
            {
                Text = macroPage.ViewModel?.Instance.Title,
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
            var macroPage = (MacroPage)page;
            if (macroPage.ViewModel.Instance.Icon is not null)
            {
                return IconInfoHelper.ConvertToIconSource(macroPage.ViewModel.Instance.Icon);
            }
            return IconInfoHelper.ConvertToIconSource(new UriIcon(MacroInstanceManager.DefaultIconUri));
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
        if (ViewModel.Instance.Template is null)
        {
            return;
        }
        if (ViewModel.Instance.Template.ConfigVariations.Count == 0)
        {
            return;
        }

        RunMenuFlyout.Items.Add(new MenuFlyoutSeparator());

        for (var i = 0; i < ViewModel.Instance.Template.ConfigVariations.Count; i++)
        {
            var variation = ViewModel.Instance.Template.ConfigVariations[i];

            if (variation.IsDevelopmentOnly && !App.Current.IsDevelopment)
            {
                continue;
            }

            var mfi = new MenuFlyoutItem()
            {
                Text = variation.Title ?? App.Localize($"Poltergeist/Macros/Run_Variation", i + 1),
                Command = ViewModel.StartCommand,
                CommandParameter = new MacroStartArguments()
                {
                    IncognitoMode = variation.IncognitoMode,
                    OptionOverrides = variation.OptionOverrides,
                    EnvironmentOverrides = variation.EnvironmentOverrides,
                    Inputs = variation.Inputs,
                },
                Icon = IconInfoHelper.ConvertToIconElement(variation.Icon is not null ? IconInfo.FromString(variation.Icon) : new GlyphIcon("\uE768")),
            };
            RunMenuFlyout.Items.Add(mfi);
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

        return true;
    }

    public void OnPageClosed()
    {
        ViewModel.SaveOptions(true);
    }

    private void StopMenuFlyout_Opening(object sender, object e)
    {
        var flyout = (MenuFlyout)sender;
        var terminateMFI = flyout.Items[0];

        flyout.Items.Clear();
        flyout.Items.Add(terminateMFI);

        if (ViewModel.Instance.Template?.Interventions?.Count > 0)
        {
            flyout.Items.Add(new MenuFlyoutSeparator());

            foreach (var intervention in ViewModel.Instance.Template.Interventions)
            {
                if (intervention.IsDevelopmentOnly && !App.Current.IsDevelopment)
                {
                    continue;
                }

                var mfi = new MenuFlyoutItem()
                {
                    Text = intervention.Title,
                    Command = ViewModel.InterveneCommand,
                    CommandParameter = intervention.Key,
                    Icon = IconInfoHelper.ConvertToIconElement(intervention.Icon ?? new GlyphIcon("\uE835")),
                };
                if (intervention.Description is not null)
                {
                    ToolTipService.SetToolTip(mfi, intervention.Description);
                }
                flyout.Items.Add(mfi);
            }
        }
    }
}
