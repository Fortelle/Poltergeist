using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Poltergeist.Automations.Macros;
using Poltergeist.Services;
using Poltergeist.ViewModels;

namespace Poltergeist.Views;

public sealed partial class MacroConsolePage : Page
{
    public MacroConsoleViewModel ViewModel { get; }

    public MacroConsolePage(MacroConsoleViewModel vm)
    {
        ViewModel = vm;
        ViewModel.UseStatistics = App.GetSettings<bool>("macro.usestatistics", true);

        InitializeComponent();
    }

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        var action = ((Hyperlink)sender).DataContext as MacroMaintenance;
        action.Execute(ViewModel.Macro);
    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        var x = ViewModel.Macro;
        ViewModel.Macro = null;
        ViewModel.Macro = x;
    }

    private void TabControl_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
    {
    }

}
