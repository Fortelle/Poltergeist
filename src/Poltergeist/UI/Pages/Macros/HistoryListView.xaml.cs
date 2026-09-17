using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.UI.Pages.Macros;

public sealed partial class HistoryListView : UserControl
{
    public static readonly DependencyProperty PrivateFolderProperty = DependencyProperty.RegisterAttached(nameof(PrivateFolder), typeof(object), typeof(HistoryListView), new PropertyMetadata(null));
    public string? PrivateFolder
    {
        get => (string?)GetValue(PrivateFolderProperty);
        set => SetValue(PrivateFolderProperty, value);
    }

    public static readonly DependencyProperty HistoryProperty = DependencyProperty.RegisterAttached(nameof(History), typeof(object), typeof(HistoryListView), new PropertyMetadata(null));
    public ProcessorHistoryEntry[]? History
    {
        get => (ProcessorHistoryEntry[]?)GetValue(HistoryProperty);
        set => SetValue(HistoryProperty, value);
    }

    public HistoryListView()
    {
        InitializeComponent();
    }

    private void HistoryListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(PrivateFolder))
        {
            return;
        }

        if (((FrameworkElement)e.OriginalSource).DataContext is not ProcessorHistoryEntry historyEntry)
        {
            return;
        }

        var logFile = Path.Combine(PrivateFolder, "Logs", historyEntry.ProcessorId + ".log");
        if (!File.Exists(logFile))
        {
            App.ShowTeachingTip(App.Localize($"Poltergeist/Macros/LogNotExist"));
            return;
        }

        var process = new Process
        {
            StartInfo = new(logFile)
            {
                UseShellExecute = true
            }
        };
        process.Start();
    }

}
