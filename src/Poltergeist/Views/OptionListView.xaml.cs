using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views;

[INotifyPropertyChanged]
public sealed partial class OptionListView : UserControl
{
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.RegisterAttached("Options", typeof(OptionCollection), typeof(OptionListView), new PropertyMetadata(null));
    private static readonly string UncategorizedGroupLabel = App.Localize("Poltergeist/Resources/Options_Uncategorized");

    public OptionCollection Options
    {
        get => (OptionCollection)GetValue(OptionsProperty);
        set
        {
            SetValue(OptionsProperty, value);

            Groups = value
                .Where(x => x.IsBrowsable)
                .GroupBy(x => x.Category)
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
        this.InitializeComponent();
    }

    public class OptionGroup
    {
        public required string Title { get; set; }
        public required IOptionItem[] Options { get; set; }
    }

}
