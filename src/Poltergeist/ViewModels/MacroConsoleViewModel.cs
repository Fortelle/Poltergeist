using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Processors.Events;
using Poltergeist.Services;
using Poltergeist.Automations.Configs;
using System.Collections.Generic;

namespace Poltergeist.ViewModels;

public class MacroConsoleViewModel : ObservableRecipient
{
    private IMacroBase _macro;
    private MacroProcessor _processor;
    private bool _isRunning;
    private MacroOptions _selectedOptions;
    private VariableCollection _statistics;
    private MacroManager MacroManager;

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }
    public IMacroBase Macro { get => _macro; set => SetProperty(ref _macro, value); }
    public MacroProcessor Processor { get => _processor; set => SetProperty(ref _processor, value); }
    public MacroOptions UserOptions { get => _selectedOptions; set => SetProperty(ref _selectedOptions, value); }
    public VariableCollection Statistics { get => _statistics; set => SetProperty(ref _statistics, value); }

    public ObservableCollection<PanelTabItem> Panels { get; set; }
    public bool UseStatistics { get; set; }

    public event Action Started;

    public MacroConsoleViewModel(MacroManager macroManager)
    {
        MacroManager = macroManager;

        Panels = new();

        StartCommand = new RelayCommand(Start);
        StopCommand = new RelayCommand(Stop);
    }

    public void Start()
    {
        Start(LaunchReason.ByUser);
    }

    public void Start(LaunchReason reason)
    {
        if (Macro is null)
        {
            return;
        }

        Keyboard.ClearFocus();

        IsRunning = true;
        MacroManager.IsRunning = true;
        MacroManager.AddRecentMacro(Macro);

        App.GetService<NavigationService>().Navigate("console");

        Processor = new MacroProcessor(Macro, reason)
        {
            WaitUiReady = true,
            Options = GetOptions(),
            Environments = GetEnvironments(),
        };
        Processor.Starting += Processor_Starting;
        Processor.Started += Processor_Started;
        Processor.Completed += Processor_Completed;
        Processor.PanelCreated += Processor_PanelCreated;

        Processor.Launch();
    }

    public void Stop()
    {
        if (Processor is null)
        {
            return;
        }

        Processor.Abort();
    }

    public void Load(IMacroBase macro)
    {
        Unload();

        Macro = macro;
        macro.Load();

        UserOptions = macro.UserOptions;
        Statistics = macro.Statistics;

        Panels.Clear();
        Panels.Add(new PanelTabItem()
        {
            Name = "poltergeist-instruments",
            Header = "Dashboard",
            Content = new InstrumentPanelControl(),
        });
        Panels.Add(new PanelTabItem()
        {
            Name = "poltergeist-logger",
            Header = "Log",
            Content = new TextPanelControl(),
        });
    }

    public void Unload()
    {
        if (Macro is null)
        {
            return;
        }

        Macro.SaveOptions();
    }

    private Dictionary<string, object> GetOptions()
    {
        var macroManager = App.GetService<MacroManager>();

        var globalOptions = macroManager.GlobalOptions.ToDictionary();
        var groupOptions = Macro.Group?.Options.ToDictionary();
        var macroOptions = Macro.UserOptions.ToDictionary();

        if (groupOptions != null)
        {
            foreach(var (key, value) in groupOptions)
            {
                if (!macroOptions.ContainsKey(key))
                {
                    macroOptions.Add(key, value);
                }
            }
        }

        foreach (var (key, value) in globalOptions)
        {
            if (!macroOptions.ContainsKey(key))
            {
                macroOptions.Add(key, value);
            }
        }

        return macroOptions;
    }

    private Dictionary<string, object> GetEnvironments()
    {
        var dict = new Dictionary<string, object>();

        var localSettings = App.GetService<LocalSettingsService>();
        dict.Add("logger.tofile", localSettings.GetSetting<LogLevel>("logger.tofile"));
        dict.Add("logger.toconsole", localSettings.GetSetting<LogLevel>("logger.toconsole"));
        dict.Add("macro.usestatistics", localSettings.GetSetting<bool>("macro.usestatistics"));

        return dict;
    }

    private void Processor_Completed(object? sender, MacroCompletedEventArgs e)
    {
        Processor.Starting -= Processor_Starting;
        Processor.Started -= Processor_Started;
        Processor.Completed -= Processor_Completed;
        Processor.PanelCreated -= Processor_PanelCreated;
        Processor.Dispose();
        Processor = null;

        IsRunning = false;
        App.GetService<MacroManager>().IsRunning = false;

        Macro.SaveOptions();

        Statistics = null;
        Statistics = Macro.Statistics;

        if(e.CompleteAction == CompleteAction.RestoreApplication)
        {
            App.GetService<ActionService>().RestoreApplication();
        }
        else if (e.IsSucceeded && e.CompleteAction != CompleteAction.None && e.Summary.Duration.TotalSeconds >= 15) // todo: config
        {
            App.GetService<ActionService>().Execute(e.CompleteAction, e.ActionArgument);
        }
    }

    private void Processor_Starting(object? sender, MacroStartingEventArgs e)
    {

    }

    private void Processor_Started(object? sender, MacroStartedEventArgs e)
    {
        if(e.Started && e.StartedActions != null)
        {
            foreach(var action in e.StartedActions)
            {
                switch (action)
                {
                    case StartedAction.MinimizedWindow:
                        App.GetService<ActionService>().MinimizeApplication();
                        break;
                }
            }
        }

        Started?.Invoke();
    }

    private void Processor_PanelCreated(object? sender, PanelCreatedEventArgs e)
    {
        var tabitem = Panels.FirstOrDefault(x => x.Name == e.Item.Key);
        if (tabitem is null)
        {
            var ctl = e.Item.CreateControl();
            ctl.DataContext = e.Item.CreateViewModel();

            tabitem = new PanelTabItem()
            {
                Name = e.Item.Key,
                Header = e.Item.Header,
                Content = ctl,
            };
            Panels.Add(tabitem);
        }
        else
        {
            tabitem.Content.DataContext = e.Item.CreateViewModel();
        }
    }

}

public class PanelTabItem
{
    public string Name { get; set; }
    public string Header { get; set; }
    public UserControl Content { get; set; }
}
