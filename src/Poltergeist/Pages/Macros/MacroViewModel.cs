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
using Poltergeist.Automations.Processors.Events;
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
    public void Start()
    {
        Start(new MacroStartArguments() {
            MacroKey = Macro.Key,
            Reason = LaunchReason.ByUser,
        });
    }

    public void Start(MacroStartArguments args)
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

        IsRunning = true;
        OnPropertyChanged(nameof(IsRunnable));

        Macro.UserOptions.Save();

        var macroManager = App.GetService<MacroManager>();

        Processor = macroManager.CreateProcessor(Macro, args.Reason);

        if (args.OptionOverrides?.Count > 0)
        {
            foreach (var (key, value) in args.OptionOverrides)
            {
                if (Processor.Options.Contains(key))
                {
                    Processor.Options[key].Value = value;
                    Processor.Options[key].Source = ParameterSource.MacroOverride;
                }
                else
                {
                    Processor.Options.Add(key, value, ParameterSource.MacroOverride);
                }
            }
        }

        Processor.Starting += Processor_Starting;
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

    private void Processor_Completed(object? sender, MacroCompletedEventArgs e)
    {
        Processor!.Starting -= Processor_Starting;
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
            _ = DialogService.ShowMessage(e.Exception.Message, Macro.Title);
        }

        Timer?.Stop();
        Timer = null;
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

    private void Processor_Starting(object? sender, MacroStartingEventArgs e)
    {
        // There is an error of about 1 second.
        // It can be avoided by changing interval to .1s, but it's not necessary.
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

    private void Processor_Started(object? sender, MacroStartedEventArgs e)
    {
        if (e.Started && e.StartedActions != null)
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
