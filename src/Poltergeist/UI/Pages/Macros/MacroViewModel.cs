using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppNotifications.Builder;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Helpers;
using Poltergeist.Modules.Instruments;
using Poltergeist.Modules.Interactions;
using Poltergeist.Modules.Macros;
using Poltergeist.UI.Controls.Instruments;

namespace Poltergeist.UI.Pages.Macros;

// todo: IsRunning does not work on ProgressRing with --autostart

public partial class MacroViewModel : ObservableRecipient
{
    private const int MaxReportCount = 100;
    private const int UserOptionsSaveDelaySeconds = 30;
    private const int CompleteActionDelaySeconds = 1;
    private const int ShutdownDelaySeconds = 15;
    private const string InstanceIdKey = "macro_processor_id";

    public ObservableCollection<PanelViewModel> Panels { get; } = new();

    public MacroInstance Instance { get; }

    public ObservableParameterCollection? UserOptions { get; }

    [ObservableProperty]
    public partial KeyValuePair<string, string>[]? Statistics { get; set; }

    [ObservableProperty]
    public partial KeyValuePair<string, string>[]? Metadata { get; set; }

    [ObservableProperty]
    public partial ProcessorHistoryEntry[]? History { get; set; }

    [ObservableProperty]
    public partial ImageSource? Thumbnail { get; set; }

    [ObservableProperty]
    public partial TimeSpan Duration { get; set; }

    [ObservableProperty]
    public partial string? ExceptionMessage { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial PanelViewModel? SelectedPanel { get; set; }

    public string? InvalidationMessage { get; }

    public bool IsValid => string.IsNullOrEmpty(InvalidationMessage);

    private DispatcherTimer? Timer;

    private IFrontProcessor? Processor;

    private Debouncer? OptionSaveDebouncer;

    public MacroViewModel(MacroInstance instance)
    {
        Instance = instance;

        instance.Load();

        if (instance.Template?.CheckValidity(out var invalidationMessage) == false)
        {
            InvalidationMessage = invalidationMessage;
        }

        if (instance.Options is not null)
        {
            UserOptions = new ObservableParameterCollection(instance.Options);
            if (Instance.IsPersistent)
            {
                UserOptions.Changed += (key, oldValue, newValue) =>
                {
                    SaveOptions();
                };
            }
        }

        Thumbnail = instance.GetThumbnail();

        RefreshMetadata();

        RefreshData();

        RefreshControls();
    }
    
    private void RefreshData()
    {
        RefreshStatistics();
        RefreshHistory();
    }

    private void RefreshStatistics()
    {
        if (Instance.Template is null)
        {
            return;
        }
        if (Instance.Statistics is null)
        {
            return;
        }

        try
        {
            Statistics = Instance.GetStatistics().ToArray();
        }
        catch
        {
        }
    }

    private void RefreshHistory()
    {
        if (Instance.Reports is null)
        {
            return;
        }

        try
        {
            var list = new List<ProcessorHistoryEntry>();
            foreach (var report in Instance.Reports)
            {
                list.Add(new ProcessorHistoryEntry()
                {
                    MacroKey = report.GetValueOrDefault<string>("macro_key"),
                    ProcessorId = report.GetValueOrDefault<string>("processor_id"),
                    StartTime = report.GetValueOrDefault<DateTime>("start_time"),
                    EndTime = report.GetValueOrDefault<DateTime>("end_time"),
                    Duration = report.GetValueOrDefault<TimeSpan>("run_duration"),
                    EndReason = report.GetValueOrDefault<EndReason>("end_reason"),
                    Comment = report.GetValueOrDefault<string>("comment_message"),
                });
            }
            History = list
                .OrderByDescending(x => x.StartTime)
                .Take(MaxReportCount)
                .ToArray();
        }
        catch
        {
        }
    }

    private void RefreshControls()
    {
    }

    private void RefreshMetadata()
    {
        var assembly = Instance.Template?.GetType().Assembly;
        var assemblyName = assembly?.GetName();

        var metadata = new Dictionary<string, object?>() {
            { "template_key", Instance.TemplateKey },
            { "template_version", Instance.Template?.Version },
            { "assembly_name", assemblyName?.Name },
            { "assembly_version", assemblyName?.Version },
            { "assembly_location", assembly?.Location },
            { "instance_id", Instance.InstanceId },
            { "instance_created_time", Instance.Properties?.CreatedTime },
            { "instance_private_folder", Instance.PrivateFolder },
        };

        var list = new List<KeyValuePair<string, string>>();
        foreach(var (key, value) in metadata)
        {
            var label = App.Localize($"Poltergeist/Macros/MetadataLabel_{key}");
            if (string.IsNullOrEmpty(label))
            {
                label = key;
            }
            var text = value?.ToString() ?? "-";
            list.Add(new (label, text));
        }

        if (Instance.Template is not null)
        {
            foreach (var prop in Instance.Template.Metadata)
            {
                if (prop.Status == ParameterStatus.DevelopmentOnly && !App.Current.IsDevelopment)
                {
                    continue;
                }

                var label = prop.DisplayLabel ?? prop.Key;
                var text = prop.FormatValue(prop.DefaultValue);
                list.Add(new(label, text));
            }
        }

        Metadata = list.ToArray();
    }

    [RelayCommand]
    public void Start(MacroStartArguments? args = null)
    {
        Start(args, LaunchReason.Manual);
    }

    public void Start(MacroStartArguments? args, LaunchReason reason)
    {
        if (!IsValid)
        {
            return;
        }

        if (Instance.Template is null)
        {
            return;
        }

        if (IsRunning)
        {
            return;
        }

        App.TryEnqueue(() =>
        {
            IsRunning = true;

            Duration = default;

            RefreshControls();
        });

        SaveOptions(true);

        args ??= Instance.DefaultStartArguments;

        var macroManager = App.GetService<MacroManager>();

        Processor = macroManager.CreateProcessor(Instance, args, reason);

        Processor.Launched += Processor_Launched;
        Processor.Completed += Processor_Completed;
        Processor.PanelCreated += Processor_PanelCreated;
        Processor.Interacting += Processor_Interacting;

        try
        {
            macroManager.Launch(Processor, Instance);
        }
        catch (Exception exception)
        {
            App.ShowException(exception);

            App.TryEnqueue(() =>
            {
                ExceptionMessage = exception.Message;
            });

            CleanUp();
        }
    }

    [RelayCommand]
    public void Stop()
    {
        if (Processor is null)
        {
            return;
        }

        Processor.Stop(AbortReason.User);
    }

    [RelayCommand]
    public void Terminate()
    {
        if (Processor is null)
        {
            return;
        }

        Processor.Terminate();
    }

    private void Processor_Launched(object? sender, ProcessorLaunchedEventArgs e)
    {
        App.TryEnqueue(() =>
        {
            ExceptionMessage = null;

            Timer = new()
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            Timer.Tick += Timer_Tick;
            Timer.Start();
        });
    }

    private void Processor_Completed(object? sender, ProcessorCompletedEventArgs e)
    {
        App.TryEnqueue(() =>
        {
            if (e.Result.Output.TryGetValue("complete_action", out var x) && x is CompletionAction action && action != CompletionAction.None)
            {
                Task.Delay(TimeSpan.FromSeconds(CompleteActionDelaySeconds)).ContinueWith(_ =>
                {
                    switch (action)
                    {
                        case CompletionAction.ExitApplication:
                            ApplicationHelper.Exit();
                            break;
                        case CompletionAction.LockScreen:
                            SystemHelper.LockScreen();
                            break;
                        case CompletionAction.ShutdownSystem:
                        case CompletionAction.RestartSystem:
                        case CompletionAction.HibernateSystem:
                        case CompletionAction.LogOffSystem:
                            Shutdown(e.Reason, e.Result, action);
                            break;
                        case CompletionAction.RestoreApplication:
                            ApplicationHelper.BringToFront();
                            break;
                        case CompletionAction.NotifyMe:
                            Notify(e.Reason, e.Result);
                            break;
                    }
                });
            }

            if (e.Result.Exception is not null)
            {
                ExceptionMessage = e.Result.Exception.Message;
            }
        });

        CleanUp();
    }

    private void Shutdown(EndReason reason, ProcessorResult result, CompletionAction action)
    {
        var conversationId = Guid.NewGuid().ToString();

        var builder = CreateAppNotificationBuilder(reason, result)
            .AddArgument(AppNotificationService.ConversationIdKey, conversationId)
            .AddArgument(InstanceIdKey, Instance.InstanceId)
            .AddText(App.Localize($"Poltergeist/Macros/Notification_{action}_message"))
            .SetDuration(AppNotificationDuration.Long)
            .AddButton(new AppNotificationButton(App.Localize($"Poltergeist/Macros/Notification_{action}_button"))
                .AddArgument(AppNotificationService.ConversationIdKey, conversationId)
                .AddArgument(InstanceIdKey, Instance.InstanceId)
                .AddArgument("shutdown", action.ToString())
                )
            .AddButton(new AppNotificationButton(App.Localize($"Poltergeist/Macros/Notification_Cancel_button"))
                .AddArgument(AppNotificationService.ConversationIdKey, conversationId)
                .AddArgument(InstanceIdKey, Instance.InstanceId)
                )
            ;

        var delay = new DelayHelper(TimeSpan.FromSeconds(ShutdownDelaySeconds), () =>
        {
            switch (action)
            {
                case CompletionAction.ShutdownSystem:
                    SystemHelper.Shutdown();
                    break;
                case CompletionAction.RestartSystem:
                    SystemHelper.Restart();
                    break;
                case CompletionAction.HibernateSystem:
                    SystemHelper.Hibernate();
                    break;
                case CompletionAction.LogOffSystem:
                    SystemHelper.LogOff();
                    break;
            }
        });

        PoltergeistApplication.GetService<AppNotificationService>().Show(builder, conversationId, (args) =>
        {
            if (args.ContainsKey("shutdown"))
            {
                delay.TriggerNow();
            }
            else
            {
                delay.Cancel();
            }
        });

        delay.Start();
    }

    private void Notify(EndReason reason, ProcessorResult result)
    {
        var builder = CreateAppNotificationBuilder(reason, result);

        PoltergeistApplication.GetService<AppNotificationService>().Show(builder);
    }

    private AppNotificationBuilder CreateAppNotificationBuilder(EndReason reason, ProcessorResult result)
    {
        var builder = new AppNotificationBuilder()
            .AddText(Instance.Title + ": " + App.Localize($"Poltergeist/Macros/EndReason_{reason}"))
            .SetAppLogoOverride(new Uri(Path.Combine(AppContext.BaseDirectory, "Poltergeist/Assets/macro_48px.png")))
            ;

        var comment = result.Report.GetValueOrDefault<string>("comment_message");
        if (!string.IsNullOrEmpty(comment))
        {
            builder.AddText(comment);
        }

        if (reason != EndReason.Complete)
        {
            builder.SetScenario(AppNotificationScenario.Urgent);
        }

        return builder;
    }

    private void CleanUp()
    {
        App.TryEnqueue(() =>
        {
            if (Timer is not null)
            {
                Timer.Stop();
                Timer.Tick -= Timer_Tick;
                Timer = null;
            }

            IsRunning = false;

            RefreshData();

            RefreshControls();
        });

        ReleaseProcessor();
    }

    private void ReleaseProcessor()
    {
        if (Processor is null)
        {
            return;
        }

        Processor.Launched -= Processor_Launched;
        Processor.Completed -= Processor_Completed;
        Processor.PanelCreated -= Processor_PanelCreated;
        Processor.Interacting -= Processor_Interacting;

        Processor.Dispose();
        Processor = null;
    }

    private void Timer_Tick(object? sender, object e)
    {
        Duration += ((DispatcherTimer)sender!).Interval;
    }

    private void Processor_PanelCreated(object? sender, PanelCreatedEventArgs e)
    {
        App.TryEnqueue(() =>
        {
            var panelVM = new PanelViewModel()
            {
                Key = e.Item.Key,
                Header = e.Item.Header,
                Instruments = new(e.Item.Instruments, ModelToViewModel, App.Current.DispatcherQueue)
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

            if (e.Item.Active)
            {
                SelectedPanel = panelVM;
            }
        });
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
        PoltergeistApplication.TryEnqueue(() =>
        {
            switch (e.Model)
            {
                case TipModel tipModel:
                    tipModel.Title = string.IsNullOrEmpty(tipModel.Title) ? Instance.Title : $"{tipModel.Title} ({Instance.Title})";
                    break;
                case DialogModel dialogModel:
                    dialogModel.Title = string.IsNullOrEmpty(dialogModel.Title) ? Instance.Title : $"{dialogModel.Title} ({Instance.Title})";
                    break;
            }

            _ = InteractionHelper.Interact(e.Model);
        });
    }

    public void SaveOptions(bool forceExecute = false)
    {
        if (!Instance.IsPersistent)
        {
            return;
        }

        if (forceExecute)
        {
            Instance.Options?.Save();
        }
        else
        {
            OptionSaveDebouncer ??= new(() => Instance.Options?.Save(), TimeSpan.FromSeconds(UserOptionsSaveDelaySeconds));
            OptionSaveDebouncer.Trigger(forceExecute);
        }
    }
}
