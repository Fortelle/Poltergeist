using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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
    private IMacroBase _macro;

    [ObservableProperty]
    private MacroProcessor? _processor;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private OptionCollection _userOptions;

    [ObservableProperty]
    private IParameterEntry[]? _statistics;

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

    public MacroViewModel(IMacroBase macro)
    {
        Macro = macro;
        Macro.Initialize();
        Macro.Load();

        InvalidationMessage = macro.CheckValidity();

        UserOptions = macro.UserOptions;

        var thumbfile = macro.GetThumbnailFile();
        if (thumbfile is not null)
        {
            var uri = new Uri(thumbfile);
            var bmp = new BitmapImage(uri);
            Thumbnail = bmp;
        }

        Refresh();
    }

    public bool IsFavorite
    {
        get
        {
            var macroManager = App.GetService<MacroManager>();
            var summary = macroManager.GetSummary(Macro.Key);
            return summary?.IsFavorite ?? false;
        }
        set
        {
            var macroManager = App.GetService<MacroManager>();
            macroManager.UpdateSummary(Macro.Key, x =>
            {
                x.IsFavorite = value;
            });
            OnPropertyChanged(nameof(IsFavorite));
        }
    }

    public void Refresh()
    {
        UpdateStatistics();
        UpdateHistory();

        OnPropertyChanged(nameof(IsRunnable));
    }

    [RelayCommand]
    public void Start(MacroStartArguments? args = null)
    {
        if (Macro is null)
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

        Macro.UserOptions.Save();

        args ??= new MacroStartArguments()
        {
            MacroKey = Macro.Key,
            Reason = LaunchReason.ByUser,
        };

        var macroManager = App.GetService<MacroManager>();
        var processor = macroManager.CreateProcessor(Macro, args.Reason);

        if (processor.Exception is not null)
        {
            App.ShowTeachingTip(processor.Exception.Message);
            return;
        }

        Processor = processor;

        IsRunning = true;
        OnPropertyChanged(nameof(IsRunnable));

        if (args.Variation is not null)
        {
            if (args.Variation.Normalized)
            {
                foreach (var option in Macro.UserOptions)
                {
                    Processor.Options.Set(option.Key, option.Default);
                }
            }

            if (args.Variation.Options?.Count > 0)
            {
                foreach (var (key, value) in args.Variation.Options)
                {
                    Processor.Options.Set(key, value, ParameterSource.MacroOverride);
                }
            }

            if (args.Variation.Environments?.Count > 0)
            {
                foreach (var (key, value) in args.Variation.Environments)
                {
                    Processor.Environments.Set(key, value, ParameterSource.MacroOverride);
                }
            }

            if (args.Variation.SessionStorage?.Count > 0)
            {
                foreach (var (key, value) in args.Variation.SessionStorage)
                {
                    Processor.SessionStorage.Set(key, value, ParameterSource.MacroOverride);
                }
            }
        }

        Processor.Launched += Processor_Launched;
        Processor.Started += Processor_Started;
        Processor.Completed += Processor_Completed;
        Processor.PanelCreated += Processor_PanelCreated;
        Processor.Interacting += Processor_Interacting;

        macroManager.TryStart(Processor);

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

    [RelayCommand]
    public void Favorite()
    {
        IsFavorite = !IsFavorite;
    }

    private void Processor_Completed(object? sender, ProcessorCompletedEventArgs e)
    {
        Processor!.Launched -= Processor_Launched;
        Processor.Started -= Processor_Started;
        Processor.Completed -= Processor_Completed;
        Processor.PanelCreated -= Processor_PanelCreated;
        Processor.Interacting -= Processor_Interacting;
        Processor = null;

        IsRunning = false;

        Refresh();

        if (e.CompleteAction != CompletionAction.None)
        {
            App.GetService<ActionService>().Execute(e.CompleteAction, e.ActionArgument);
        }

        if(e.Exception != null)
        {
            ExceptionMessage = e.Exception.Message;
        }

        Timer?.Stop();
        Timer = null;
        Processor = null;
    }

    private void UpdateStatistics()
    {
        Statistics = null;
        Statistics = Macro.Statistics.ToArray();
    }

    private void UpdateHistory()
    {
        History = null;
        History = Macro.History.Take(100);
    }

    private void Processor_Launched(object? sender, ProcessorLaunchedEventArgs e)
    {
        // There is an error of about 1 second.
        // It can be avoided by changing interval to .1s, but it's not necessary.
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

    private void Processor_Started(object? sender, ProcessorStartedEventArgs e)
    {
        if (e.StartedActions?.Length > 0)
        {
            foreach (var action in e.StartedActions)
            {
                switch (action)
                {
                    case StartedAction.MinimizedWindow:
                        ActionService.MinimizeApplication();
                        break;
                }
            }
        }
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
                tipModel.Title = string.IsNullOrEmpty(tipModel.Title) ? Macro.Title : $"{tipModel.Title} ({Macro.Title})";
                break;
            case DialogModel dialogModel:
                dialogModel.Title = string.IsNullOrEmpty(dialogModel.Title) ? Macro.Title : $"{dialogModel.Title} ({Macro.Title})";
                break;
        }
        _ = App.Interact(e.Model);
    }

}
