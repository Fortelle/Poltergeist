using Microsoft.UI.Xaml.Controls;
using Poltergeist.Modules.Logging;

namespace Poltergeist.UI.Pages.Logging;

public sealed partial class LoggingPage : Page, IPageClosed
{
    public LoggingViewModel ViewModel { get; }

    public LoggingPage(LoggingViewModel viewmodel)
    {
        ViewModel = viewmodel;

        InitializeComponent();
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listview = (ListView)sender;
        if (listview.SelectedItems.Count > 1)
        {
            var logEntries = listview.SelectedItems.OfType<AppLogEntry>().ToArray();
            var duration = logEntries[^1].Timestamp - logEntries[0].Timestamp;
            ViewModel.TotalTime = duration.TotalMilliseconds + "ms";
        }
        else
        {
            ViewModel.TotalTime = null;
        }
    }

    public void OnPageClosed()
    {
        ViewModel.Dispose();
    }
}
