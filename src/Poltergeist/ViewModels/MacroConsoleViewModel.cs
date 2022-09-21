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
    private MacroBase _macro;
    private MacroProcessor _processor;
    private bool _isRunning;
    private MacroOptions _selectedOptions;
    private VariableCollection _environments;

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }
    public MacroBase Macro { get => _macro; set => SetProperty(ref _macro, value); }
    public MacroProcessor Processor { get => _processor; set => SetProperty(ref _processor, value); }
    public MacroOptions UserOptions { get => _selectedOptions; set => SetProperty(ref _selectedOptions, value); }
    public VariableCollection Environments { get => _environments; set => SetProperty(ref _environments, value); }

    public ObservableCollection<PanelTabItem> Panels { get; set; }

    public event Action Started;

    public MacroConsoleViewModel()
    {
        Panels = new();

        StartCommand = new RelayCommand(Start);
        StopCommand = new RelayCommand(Stop);
    }

    public void Start()
    {
        if (Macro is null)
        {
            return;
        }

        IsRunning = true;
        App.GetService<MacroManager>().IsRunning = true;

        App.GetService<NavigationService>().Navigate("console");

        var options = GetOptions(Macro);

        Processor = new MacroProcessor(Macro, LaunchReason.ByUser, options)
        {
            WaitUiReady = true,
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

    public void Load(MacroBase macro)
    {
        Unload();

        Macro = macro;
        UserOptions = macro.UserOptions;
        Environments = macro.Environments;

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

    private Dictionary<string, object> GetOptions(MacroBase macro)
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

        Environments = null;
        Environments = Macro.Environments;
    }

    private void Processor_Starting(object? sender, MacroStartedEventArgs e)
    {

    }

    private void Processor_Started(object? sender, EventArgs e)
    {
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
