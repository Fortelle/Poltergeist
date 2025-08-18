using System.Diagnostics;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Modules.Settings;

namespace Poltergeist.UI.Windows;

public partial class PerformanceViewModel : ObservableRecipient
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [ObservableProperty]
    public partial string? Text { get; set; }

    private System.Timers.Timer? UpdateTimer;
    private PerformanceCounter? CpuCounter;
    private PerformanceCounter? RamCounter;

    public PerformanceViewModel()
    {
        var settingsService = App.GetService<AppSettingsService>();

        var showPerformance = settingsService.Settings.Get<bool>("app_show_performance");
        if (showPerformance)
        {
            TogglePerformance(true);
        }

        settingsService.WatchChange<bool>("app_show_performance", TogglePerformance);
    }

    private void TogglePerformance(bool show)
    {
        if (show)
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            CpuCounter = new PerformanceCounter("Process", "% Processor Time", processName);
            RamCounter = new PerformanceCounter("Process", "Working Set", processName);
            UpdateTimer = new System.Timers.Timer(UpdateInterval);
            UpdateTimer.Elapsed += Timer_Elapsed;
            UpdateTimer.Start();
        }
        else
        {
            UpdateTimer?.Elapsed -= Timer_Elapsed;
            UpdateTimer?.Dispose();
            CpuCounter?.Dispose();
            RamCounter?.Dispose();
            Text = "";
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (!App.Current.IsReady)
        {
            return;
        }

        App.TryEnqueue(UpdatePerformance);
    }

    private void UpdatePerformance()
    {
        var cpuValue = CpuCounter?.NextValue();
        var ramValue = RamCounter?.NextValue() / 1024 / 1024;

        Text = $"CPU: {cpuValue:N2}%, RAM: {ramValue:#}MB";
    }
}