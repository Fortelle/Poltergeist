using System.Diagnostics;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Poltergeist.UI.Windows;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string? _cpuValue;

    [ObservableProperty]
    private string? _ramValue;

    protected PerformanceCounter? CpuCounter;
    protected PerformanceCounter? RamCounter;

    public bool IsDevelopment { get; }
    public bool IsAdministrator { get; }
    public bool IsExclusiveMacroMode { get; }

    private bool _showPerformance;
    public bool ShowPerformance
    {
        get => _showPerformance;
        set
        {
            SetProperty(ref _showPerformance, value);

            if (value)
            {
                Task.Run(() =>
                {
                    var processName = Process.GetCurrentProcess().ProcessName;
                    CpuCounter = new PerformanceCounter("Process", "% Processor Time", processName);
                    RamCounter = new PerformanceCounter("Process", "Working Set", processName);
                    var timer = new System.Timers.Timer(1000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                });
            }
            else
            {
                CpuCounter?.Dispose();
                RamCounter?.Dispose();
            }
        }
    }

    public ShellViewModel()
    {
        IsDevelopment = App.Current.IsDevelopment;
        IsExclusiveMacroMode = App.Current.ExclusiveMacroMode is not null;
        IsAdministrator = App.Current.IsAdministrator;
        ShowPerformance = App.Current.StartupOptions.Contains("ShowPerformance");
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        App.TryEnqueue(Update);
    }

    private void Update()
    {
        var cpuValue = CpuCounter?.NextValue();
        CpuValue = $"CPU: {cpuValue:N2}%";

        var ramValue = RamCounter?.NextValue();
        ramValue = ramValue / 1024 / 1024;
        RamValue = $"RAM: {ramValue:#}MB";
    }
}
