using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls;

[INotifyPropertyChanged]
public sealed partial class OptionListView : UserControl
{
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.RegisterAttached("Options", typeof(IEnumerable<ObservableParameterItem>), typeof(OptionListView), new PropertyMetadata(null));
    private static readonly string UncategorizedGroupLabel = App.Localize("Poltergeist/Resources/Options_Uncategorized");

    public IEnumerable<ObservableParameterItem> Options
    {
        get => (IEnumerable<ObservableParameterItem>)GetValue(OptionsProperty);
        set
        {
            SetValue(OptionsProperty, value);

            if (value is null)
            {
                return;
            }

            var childGroups = value
                .Where(x => x.Definition.Status != ParameterStatus.Hidden)
                .Where(x => x.Definition is IOptionDefinition od && od.Parent is not null)
                .GroupBy(x => ((IOptionDefinition)x.Definition).Parent)
                .ToDictionary(x => x.Key!, x => x.ToArray())
                ;

            Groups = value
                .Where(x => x.Definition.Status != ParameterStatus.Hidden)
                .Where(x => x.Definition is not IOptionDefinition od || od.Parent is null)
                .GroupBy(x => x.Definition.Category)
                .OrderBy(x => x.Key is null ? 0 : 1)
                .Select(x => new OptionGroup
                {
                    Title = x.Key ?? UncategorizedGroupLabel,
                    Options = x.Select(y => new OptionItem()
                    {
                        Item = y,
                        SubItems = childGroups.TryGetValue(y.Definition.Key, out var children) ? children : null,
                    }),
                })
                .ToArray();
        }
    }

    public static readonly DependencyProperty IsLockedProperty = DependencyProperty.RegisterAttached("IsLocked", typeof(bool), typeof(OptionListView), new PropertyMetadata(false));

    public bool IsLocked
    {
        get => (bool)GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    [ObservableProperty]
    public partial OptionGroup[]? Groups { get; set; }

    public OptionListView()
    {
        InitializeComponent();
    }

}
