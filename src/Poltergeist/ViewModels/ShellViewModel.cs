using System.Diagnostics;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;

using Poltergeist.Contracts.Services;
using Poltergeist.Services;

namespace Poltergeist.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    public NavigationInfo HomeInfo { get; }
    public NavigationInfo SettingsInfo { get; }
    public NavigationInfo AboutInfo { get; }
    public NavigationInfo DebugInfo { get; }

    [ObservableProperty]
    private string? _cpuValue;

    [ObservableProperty]
    private string? _ramValue;

    protected PerformanceCounter? CpuCounter;
    protected PerformanceCounter? RamCounter;

    public bool IsDebug { get; }
    public bool IsSingleMacroMode { get; }

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
                    var timer = new System.Timers.Timer(3000);
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

    public ShellViewModel(INavigationService navigationService)
    {
        HomeInfo = navigationService.GetInfo("home")!;
        SettingsInfo = navigationService.GetInfo("settings")!;
        AboutInfo = navigationService.GetInfo("about")!;
        DebugInfo = navigationService.GetInfo("debug")!;

        IsDebug = Debugger.IsAttached;
        IsSingleMacroMode = App.SingleMacroMode is not null;

        //todo: config
        //ShowPerformance = true;
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(Update);
    }

    private void Update()
    {
        var cpuValue = CpuCounter?.NextValue();
        CpuValue = $"{cpuValue:N2}%";

        var ramValue = RamCounter?.NextValue();
        ramValue = ramValue / 1024 / 1024;
        RamValue = $"{ramValue:#} MB";
    }
}
