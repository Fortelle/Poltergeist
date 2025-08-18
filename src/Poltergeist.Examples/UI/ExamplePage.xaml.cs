using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Modules.Macros;

namespace Poltergeist.Examples.UI;

public sealed partial class ExamplePage : Page
{
    public ExampleViewModel ViewModel { get; } = new();

    public ExamplePage()
    {
        InitializeComponent();

        ExampleCVS.Source = ViewModel.MacroInstances
            .GroupBy(x => string.IsNullOrEmpty(x.Category) ? "Default" : x.Category)
            .OrderByDescending(x => x.Key == "Default")
            .ThenBy(x => x.Key)
            ;
    }

    private void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is not MacroInstance instance)
        {
            return;
        }

        ViewModel.OpenExample(instance);
    }
}
