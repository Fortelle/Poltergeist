using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Components.Thumbnails;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;
using Poltergeist.Helpers.Converters;
using Poltergeist.Macros.Instruments;
using Poltergeist.Pages.Macros.Instruments;
using Poltergeist.Services;

namespace Poltergeist.Pages.Macros;

// todo: IsRunning does not work on ProgressRing with --autostart

public partial class MacroViewModel : ObservableRecipient
{
    public ObservableCollection<PanelViewModel> Panels { get; } = new();

    [ObservableProperty]
    private MacroShell _shell;

    [ObservableProperty]
    private IFrontProcessor? _processor;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private ObservableParameterCollection? _userOptions;

    [ObservableProperty]
    private ParameterDefinitionValuePair[]? _statistics;

    [ObservableProperty]
    private KeyValuePair<string, string>[]? _properties;

    [ObservableProperty]
    private ProcessHistoryEntry[]? _history;

    [ObservableProperty]
    private ImageSource? _thumbnail;

    [ObservableProperty]
    private string? _duration;

    [ObservableProperty]
    private string? _exceptionMessage;

    private DispatcherTimer? Timer;
    private DateTime StartTime;

    public string? InvalidationMessage { get; }
    public bool IsValid => string.IsNullOrEmpty(InvalidationMessage);
    public bool IsRunnable => IsValid && !IsRunning;

    public MacroViewModel(MacroShell shell)
    {
        Shell = shell;

        shell.Load();

        InvalidationMessage = shell.Template?.CheckValidity();

        if (shell.UserOptions is not null)
        {
            UserOptions = new ObservableParameterCollection(shell.UserOptions);
        }

        var thumbfile = shell.GetThumbnailFile();
        if (thumbfile is not null)
        {
            var uri = new Uri(thumbfile);
            var bmp = new BitmapImage(uri);
            Thumbnail = bmp;
        }

        RefreshProperties();

        Refresh();
    }

    public void Refresh()
    {
        Statistics = Shell.Statistics?.ToDefinitionValueArray();

        History = Shell.History?.Take(100);

        OnPropertyChanged(nameof(IsRunnable));
    }

    public void RefreshProperties()
    {
        var properties = new Dictionary<string, object?>(){
            {"template_key", Shell.TemplateKey},
            {"shell_key", Shell.ShellKey},
            {"template_version", Shell.Template?.Version},
            {"shell_created_time", Shell.Properties.CreatedTime},
            {"shell_private_folder", Shell.PrivateFolder},
        };

        if (Shell.Template is not null)
        {
            foreach (var prop in Shell.Template.Properties)
            {
                properties.Add(prop.Key, prop.FormatValue(prop.DefaultValue));
            }
        }

        Properties = properties.Select(x =>
        {
            var key = App.Localize($"Poltergeist/Macros/PropertyLabel_{x.Key}");
            if (string.IsNullOrEmpty(key))
            {
                key = x.Key;
            }
            var value = x.Value?.ToString() ?? "-";
            return new KeyValuePair<string, string>(key, value);
        }).ToArray();
    }

    [RelayCommand]
    public void Start(MacroStartArguments? args = null)
    {
        if (Shell.Template is null)
        {
            return;
        }

        if (IsRunning)
        {
            return;
        }

        if (!IsRunnable)
        {
            return;
        }

        Shell.UserOptions?.Save();

        args ??= new MacroStartArguments()
        {
            ShellKey = Shell.ShellKey,
            Reason = LaunchReason.ByUser,
        };

        var macroManager = App.GetService<MacroManager>();

        Processor = macroManager.CreateProcessor(args);

        IsRunning = true;
        OnPropertyChanged(nameof(IsRunnable));
        
        Processor.Launched += Processor_Launched;
        Processor.Completed += Processor_Completed;
        Processor.PanelCreated += Processor_PanelCreated;
        Processor.Interacting += Processor_Interacting;
        macroManager.MacroProcessorCompleted += MacroManager_MacroProcessorCompleted;

        macroManager.Launch(Processor);

        Duration = TimeSpanToHhhmmssConverter.ToString(default);
    }

    [RelayCommand]
    public void Stop()
    {
        if (Processor is null)
        {
            return;
        }

        Processor.Abort();
    }

    private void Processor_Completed(object? sender, ProcessorCompletedEventArgs e)
    {
        Processor!.Launched -= Processor_Launched;
        Processor.Completed -= Processor_Completed;
        Processor.PanelCreated -= Processor_PanelCreated;
        Processor.Interacting -= Processor_Interacting;

        var macroManager = App.GetService<MacroManager>();

        if (e.CompletionAction != CompletionAction.None)
        {
            App.GetService<ActionService>().Execute(e.CompletionAction);
        }

        if(e.Exception != null)
        {
            ExceptionMessage = e.Exception.Message;
        }
    }

    private void MacroManager_MacroProcessorCompleted(MacroShell shell)
    {
        var macroManager = App.GetService<MacroManager>();
        macroManager.MacroProcessorCompleted -= MacroManager_MacroProcessorCompleted;

        Processor = null;

        IsRunning = false;

        Timer?.Stop();
        Timer = null;

        Refresh();
    }

    private void Processor_Launched(object? sender, ProcessorLaunchedEventArgs e)
    {
        ExceptionMessage = null;
        StartTime = e.StartTime;
        Timer = new()
        {
            Interval = TimeSpan.FromSeconds(1), 
        };
        Timer.Tick += (s, e) =>
        {
            var duration = DateTime.Now - StartTime;
            Duration = TimeSpanToHhhmmssConverter.ToString(duration);
        };
        Timer.Start();
    }

    private void Processor_PanelCreated(object? sender, PanelCreatedEventArgs e)
    {
        var panelVM = new PanelViewModel()
        {
            Key = e.Item.Key,
            Header = e.Item.Header,
            Instruments = new(e.Item.Instruments, ModelToViewModel, App.MainWindow.DispatcherQueue)
            {
                IsFilled = e.Item.IsFilled,
            },
        };

        var tabitem = Panels.FirstOrDefault(x => x.Key == e.Item.Key);
        if (tabitem is not null)
        {
            var index = Panels.IndexOf(tabitem);
            Panels[index] = panelVM;
        }
        else
        {
            if (e.Item.ToLeft)
            {
                Panels.Insert(0, panelVM);
            }
            else
            {
                Panels.Add(panelVM);
            }
        }
    }

    private IInstrumentViewModel? ModelToViewModel(IInstrumentModel? model)
    {
        if (model is null)
        {
            return null;
        }

        var instrumentService = App.GetService<InstrumentManager>();
        var info = instrumentService.GetInfo(model);
        var viewmodel = Activator.CreateInstance(info.ViewModelType, model) as IInstrumentViewModel;
        
        return viewmodel;
    }

    private void Processor_Interacting(object? sender, InteractingEventArgs e)
    {
        switch (e.Model)
        {
            case TipModel tipModel:
                tipModel.Title = string.IsNullOrEmpty(tipModel.Title) ? Shell.Title : $"{tipModel.Title} ({Shell.Title})";
                break;
            case DialogModel dialogModel:
                dialogModel.Title = string.IsNullOrEmpty(dialogModel.Title) ? Shell.Title : $"{dialogModel.Title} ({Shell.Title})";
                break;
        }
        _ = App.Interact(e.Model);
    }
}
