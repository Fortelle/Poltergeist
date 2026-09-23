using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls.Instruments;

[DependencyProperty<IndicatorInstrumentItemViewModel>("ViewModel")]
public sealed partial class IndicatorIconView : UserControl
{
    public IndicatorIconView()
    {
        InitializeComponent();
    }

    private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (DataContext is not IndicatorInstrumentItemViewModel viewModel)
        {
            return;
        }

        ViewModel = viewModel;

        switch (viewModel.Motion)
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
