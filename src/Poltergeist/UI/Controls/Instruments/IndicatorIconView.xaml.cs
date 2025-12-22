using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls.Instruments;

public sealed partial class IndicatorIconView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.RegisterAttached(nameof(ViewModel), typeof(IndicatorInstrumentItemViewModel), typeof(IndicatorIconView), new PropertyMetadata(null));

    public IndicatorInstrumentItemViewModel ViewModel
    {
        get => (IndicatorInstrumentItemViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public IndicatorIconView()
    {
        InitializeComponent();
    }

    private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (DataContext is IndicatorInstrumentItemViewModel viewModel)
        {
            ViewModel = viewModel;
        }
        switch (ViewModel?.Motion)
        {
            case Automations.Components.Panels.IndicatorMotion.Fadeout:
                FadeoutStoryboard.Begin();
                break;
            case Automations.Components.Panels.IndicatorMotion.Blinking:
                BlinkingStoryboard.Begin();
                break;
            case Automations.Components.Panels.IndicatorMotion.Breathing:
                BreathingStoryboard.Begin();
                break;
        }
    }
}
