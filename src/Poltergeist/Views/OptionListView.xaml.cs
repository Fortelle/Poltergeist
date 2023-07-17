using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Configs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views;

[INotifyPropertyChanged]
public sealed partial class OptionListView : UserControl
{
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.RegisterAttached("Options", typeof(MacroOptions), typeof(OptionListView), new PropertyMetadata(null));

    public MacroOptions Options
    {
        get => (MacroOptions)GetValue(OptionsProperty);
        set
        {
            SetValue(OptionsProperty, value);

            OptionCVS = new CollectionViewSource()
            {
                Source = value.Where(x => x.IsBrowsable).GroupBy(x => x.Category),
                IsSourceGrouped = true,
            };
        }
    }

    public static readonly DependencyProperty IsLockedProperty = DependencyProperty.RegisterAttached("IsLocked", typeof(bool), typeof(OptionListView), new PropertyMetadata(false));

    public bool IsLocked
    {
        get => (bool)GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    [ObservableProperty]
    private CollectionViewSource? _optionCVS;

    public OptionListView()
    {
        this.InitializeComponent();
    }

}
