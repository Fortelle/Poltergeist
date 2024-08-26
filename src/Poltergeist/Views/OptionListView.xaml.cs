using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Views;

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

            Groups = value?
                .Where(x => x.Definition.Status != ParameterStatus.Hidden)
                .GroupBy(x => x.Definition.Category)
                .OrderBy(x => x.Key is null ? 0 : 1)
                .Select(x => new OptionGroup
                {
                    Title = x.Key ?? UncategorizedGroupLabel,
                    Options = x.ToArray(),
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
    private OptionGroup[]? _groups;

    public OptionListView()
    {
        InitializeComponent();
    }

    public class OptionGroup
    {
        public required string Title { get; set; }
        public required IEnumerable<ObservableParameterItem> Options { get; set; }
    }

}
