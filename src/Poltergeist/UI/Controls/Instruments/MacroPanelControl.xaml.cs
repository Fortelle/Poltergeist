using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Modules.Instruments;

namespace Poltergeist.UI.Controls.Instruments;

[DependencyProperty<InstrumentsWrapper>("Instruments")]
public sealed partial class MacroPanelControl : UserControl
{
    public MacroPanelControl()
    {
        InitializeComponent();
    }

    private void ContentControl_Loaded(object sender, RoutedEventArgs e)
    {
        var control = (ContentControl)sender;
        if (control.DataContext is not IInstrumentViewModel viewmodel)
        {
            return;
        }

        var instrumentService = App.GetService<InstrumentManager>();
        var info = instrumentService.GetInfo(viewmodel);
        var view = Activator.CreateInstance(info.ViewType, viewmodel);
        control.Content = view as UserControl;
    }

}

public class PanelTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ListTemplate { get; set; }
    public DataTemplate? SingleTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is not InstrumentsWrapper vm)
        {
            return null;
        }

        if (vm.IsFilled)
        {
            return SingleTemplate;
        }
        else
        {
            return ListTemplate;
        }
    }
}