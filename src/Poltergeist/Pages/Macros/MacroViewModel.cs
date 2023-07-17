using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Components.Thumbnails;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Processors.Events;
using Poltergeist.Helpers.Converters;
using Poltergeist.Macros.Instruments;
using Poltergeist.Notifications;
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
    private MacroOptions _userOptions;

    [ObservableProperty]
    private VariableItem[]? _statistics;

    [ObservableProperty]
    private ProcessSummary[]? _summaries;

    [ObservableProperty]
    private ImageSource? _thumbnail;

    [ObservableProperty]
    private string? _duration;

    private DispatcherTimer? Timer;
    private DateTime StartTime;

    public MacroViewModel(IMacroBase macro)
    {
        Macro = macro;
        Macro.Initialize();
        Macro.Load();

        UserOptions = macro.UserOptions;
        Statistics = macro.Statistics.ToArray();
        Summaries = macro.Summaries.OrderByDescending(x => x.StartTime).ToArray();

        var thumbfile = macro.GetThumbnailFile();
        if (thumbfile is not null)
        {
            var uri = new Uri(thumbfile);
            var bmp = new BitmapImage(uri);
            Thumbnail = bmp;
        }
    }

    public bool IsFavorite
    {
        get
        {
            var macroManager = App.GetService<MacroManager>();
            var summary = macroManager.GetSummary(Macro.Name);
            return summary?.IsFavorite ?? false;
        }
        set
        {
            var macroManager = App.GetService<MacroManager>();
            macroManager.UpdateSummary(Macro.Name, x =>
            {
                x.IsFavorite = value;
            });
            OnPropertyChanged(nameof(IsFavorite));
        }
    }

    [RelayCommand]
    public void Start()
    {
        Start(new MacroStartArguments() {
            MacroKey = Macro.Name,
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

        IsRunning = true;

        Macro.SaveOptions();

        var options = GetOptions();
        if(args.OptionOverrides?.Count > 0)
        {
            foreach(var (key, value) in args.OptionOverrides)
            {
                options[key] = value;
            }
        }

        Processor = new MacroProcessor(Macro, args.Reason)
        {
            Options = options,
            Environments = GetEnvironments(),
        };

        Processor.Starting += Processor_Starting;
        Processor.Started += Processor_Started;
        Processor.Completed += Processor_Completed;
        Processor.PanelCreated += Processor_PanelCreated;
        Processor.Interacting += Processor_Interacting;

        var macroManager = App.GetService<MacroManager>();
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

    private Dictionary<string, object?> GetOptions()
    {
        var macroManager = App.GetService<MacroManager>();

        var globalOptions = macroManager.GlobalOptions.ToDictionary();
        var groupOptions = Macro.Group?.Options.ToDictionary();
        var macroOptions = Macro.UserOptions.ToDictionary();

        if (groupOptions != null)
        {
            foreach (var (key, value) in groupOptions)
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

    private static Dictionary<string, object> GetEnvironments()
    {
        var dict = new Dictionary<string, object>();

        var localSettings = App.GetService<LocalSettingsService>();
        dict.Add("logger.tofile", localSettings.Get<LogLevel>("logger.tofile"));
        dict.Add("logger.toconsole", localSettings.Get<LogLevel>("logger.toconsole"));
        dict.Add("macro.usestatistics", localSettings.Get<bool>("macro.usestatistics"));

        return dict;
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

        Statistics = null;
        Statistics = Macro.Statistics.ToArray();
        Summaries = null;
        Summaries = Macro.Summaries.OrderByDescending(x => x.StartTime).ToArray();

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
                TipService.Show(tipModel);
                break;
            case DialogModel dialogModel:
                dialogModel.Title = string.IsNullOrEmpty(dialogModel.Title) ? Macro.Title : $"{dialogModel.Title} ({Macro.Title})";
                _ = DialogService.ShowAsync(dialogModel);
                break;
            case FileOpenModel fileOpenModel:
                _ = DialogService.ShowFileOpenPickerAsync(fileOpenModel);
                break;
            case FileSaveModel fileSaveModel:
                _ = DialogService.ShowFileSavePickerAsync(fileSaveModel);
                break;
            case FolderModel folderModel:
                _ = DialogService.ShowFolderPickerAsync(folderModel);
                break;
            case ToastModel toastModel:
                App.GetService<AppNotificationService>().Show(toastModel);
                break;
        }

    }

}
