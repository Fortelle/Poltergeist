using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Navigation;
using Poltergeist.UI.Pages.Macros;

namespace Poltergeist.Modules.Macros;

public class MacroManager : ServiceBase
{
    public const string MacroPageKeyFormat = "macro:{0}";

    public Dictionary<string, object?> GlobalEnvironments { get; }

    private ConcurrentDictionary<IFrontProcessor, MacroInstance?> InRunningProcessors { get; set; } = new();

    private readonly NavigationService NavigationService;

    private readonly MacroInstanceManager InstanceManager;

    public MacroManager(
        AppEventService eventService,
        NavigationService navigationService,
        MacroInstanceManager macroInstanceManager
        )
    {
        NavigationService = navigationService;
        InstanceManager = macroInstanceManager;

        GlobalEnvironments = GetGlobalEnvironments();

        eventService.Subscribe<AppWindowClosingEvent>(OnAppWindowClosing);
        eventService.Subscribe<AppNotificationReceivedEvent>(OnAppNotificationReceived);
    }

    public bool OpenPage(MacroInstance instance)
    {
        if (instance.Template is null)
        {
            return false;
        }

        return NavigationService.NavigateTo(instance.GetPageKey(), instance);
    }

    public bool OpenPage(MacroInstance instance, [MaybeNullWhen(false)] out MacroViewModel viewmodel)
    {
        if (!OpenPage(instance))
        {
            viewmodel = null;
            return false;
        }

        viewmodel = GetMacroViewModel(instance.InstanceId);
        return viewmodel is not null;
    }

    public bool OpenPage(string instanceId)
    {
        var instance = InstanceManager.GetInstance(instanceId);

        if (instance is null)
        {
            return false;
        }

        return OpenPage(instance);
    }

    public bool OpenPage(string instanceId, [MaybeNullWhen(false)] out MacroViewModel viewmodel)
    {
        var instance = InstanceManager.GetInstance(instanceId);

        if (instance is null)
        {
            viewmodel = null;
            return false;
        }

        return OpenPage(instance, out viewmodel);
    }

    public static string GetPageKey(string instanceId)
    {
        return string.Format(MacroPageKeyFormat, instanceId.ToLower());
    }

    private MacroViewModel? GetMacroViewModel(string instanceId)
    {
        var pageKey = GetPageKey(instanceId);
        var tabViewItem = NavigationService.TabView?.TabItems
            .OfType<TabViewItem>()
            .SingleOrDefault(x => x.Name == pageKey);
        if (tabViewItem?.Content is not MacroPage macroPage)
        {
            return null;
        }
        return macroPage.ViewModel;
    }

    public MacroProcessor CreateProcessor(MacroInstance instance, MacroStartArguments? args = null, LaunchReason reason = LaunchReason.Unknown)
    {
        ArgumentNullException.ThrowIfNull(instance.Template);

        args ??= instance.DefaultStartArguments ?? new();

        var options = new Dictionary<string, object?>();
        if (!args.IncognitoMode)
        {
            foreach (var (definition, value) in PoltergeistApplication.GetService<GlobalOptionsService>().GlobalOptions.GetDefinitionValueCollection())
            {
                options[definition.Key] = value;
            }
        }
        if (!args.IncognitoMode && instance.Options?.DefinitionCount > 0)
        {
            foreach (var (definition, value) in instance.Options.GetDefinitionValueCollection())
            {
                options[definition.Key] = value;
            }
        }
        if (args.OptionOverrides is not null)
        {
            foreach (var (key, value) in args.OptionOverrides)
            {
                options[key] = value;
            }
        }

        var environments = new Dictionary<string, object?>();
        foreach (var (key, value) in GlobalEnvironments)
        {
            environments[key] = value;
        }
        if (args.IncognitoMode)
        {
            environments[IncognitoModeExtensions.EnvironmentKey] = true;
        }

        foreach (var (key, value) in instance.GetEnvironments())
        {
            environments[key] = value;
        }
        
        if (args.EnvironmentOverrides?.Count > 0)
        {
            foreach (var (key, value) in args.EnvironmentOverrides)
            {
                environments[key] = value;
            }
        }

        var processorArguments = new MacroProcessorArguments
        {
            LaunchReason = reason,
            Options = options,
            Environments = environments,
            SessionStorage = args.SessionStorage,
        };

        var processor = new MacroProcessor((MacroBase)instance.Template, processorArguments);

        if (args.Callback is not null)
        {
            processor.Completed += (s, e) => args.Callback(e.Result);
        }

        return processor;
    }

    private static Dictionary<string, object?> GetGlobalEnvironments()
    {
        var environments = new Dictionary<string, object?>
        {
            { "application_name", PoltergeistApplication.ApplicationName },
            { "document_data_folder", PoltergeistApplication.Paths.DocumentDataFolder },
            { "shared_folder", PoltergeistApplication.Paths.SharedFolder },
            { "macro_folder", PoltergeistApplication.Paths.MacroFolder },
            { "is_development", PoltergeistApplication.Current.IsDevelopment },
            { "is_administrator", PoltergeistApplication.Current.IsAdministrator },
            { "is_exclusivemode", PoltergeistApplication.Current.ExclusiveMacroMode is not null },
        };

        return environments;
    }

    public MacroProcessor Launch(MacroInstance instance, MacroStartArguments? args = null)
    {
        var processor = CreateProcessor(instance, args);
        Launch(processor, instance);
        return processor;
    }

    public void Launch(IFrontProcessor processor, MacroInstance? instance = null)
    {
        Logger.Trace($"Launching macro processor.", new
        {
            TemplateKey = processor.Macro.Key,
            instance?.InstanceId,
            processor.ProcessorId,
            processor.Options,
            processor.Environments,
            SessionStorage = processor.SessionStorage.Keys.ToArray(),
        });

        if (InRunningProcessors.ContainsKey(processor))
        {
            throw new InvalidOperationException("The macro processor is already running.");
        }

        InRunningProcessors.TryAdd(processor, instance);

        processor.Launched += Processor_Launched;
        processor.Completed += Processor_Completed;

        Logger.Info($"Macro '{processor.Macro.Key}' started.");

        processor.Start();
    }

    private void Processor_Launched(object? sender, ProcessorLaunchedEventArgs e)
    {
        if (sender is not IFrontProcessor processor)
        {
            throw new InvalidOperationException();
        }

        processor.Launched -= Processor_Launched;

        if (!processor.IsIncognitoMode())
        {
            InRunningProcessors.TryGetValue(processor, out var instance);
            if (instance is not null)
            {
                InstanceManager.UpdateProperties(instance, settings =>
                {
                    settings.LastRunTime = DateTime.Now;
                    settings.RunCount += 1;
                });
            }
        }

        PoltergeistApplication.GetService<AppEventService>().Publish(new MacroProcessorLaunchedEvent(processor));
    }

    private void Processor_Completed(object? sender, ProcessorCompletedEventArgs e)
    {
        if (sender is not IFrontProcessor processor)
        {
            throw new InvalidOperationException();
        }
        
        processor.Completed -= Processor_Completed;

        InRunningProcessors.TryRemove(processor, out var instance);

        if (instance is not null)
        {
            if (instance.Template is null)
            {
                throw new InvalidOperationException();
            }

            if (!processor.IsIncognitoMode())
            {
                PoltergeistApplication.GetService<MacroStatisticsService>().UpdateStatistics(instance, e.Result.Report);

                if (instance.Reports is not null)
                {
                    instance.Reports.Add(e.Result.Report);
                    if (instance.IsPersistent)
                    {
                        try
                        {
                            instance.Reports.Save();
                        }
                        catch (Exception exception)
                        {
                            Logger.Warn($"Failed to save macro report: {exception.Message}");
                        }
                    }
                }
            }
        }

        PoltergeistApplication.GetService<AppEventService>().Publish(new MacroProcessorCompletedEvent()
        {
            Reason = e.Reason,
            Result = e.Result,
            Macro = processor.Macro,
            Instance = instance,
        });

        Logger.Info($"Macro '{processor.Macro.Key}' ended.");
    }

    public void SendMessage(InteractionMessage message)
    {
        var processor = InRunningProcessors.Keys.FirstOrDefault(x => x.ProcessorId == message.ProcessorId);
        if (processor is null)
        {
            Logger.Warn($"Failed to send message: Processor not found.", new
            {
                message.ProcessorId,
            });
            return;
        }

        Logger.Trace($"Sending message to processor.", new
        {
            message.ProcessorId,
        });

        processor.ReceiveMessage(message.ToDictionary());
    }

    private void OnAppWindowClosing(AppWindowClosingEvent e)
    {
        if (!InRunningProcessors.IsEmpty)
        {
            e.Cancel = true;
            e.CancelMessage = "One or more macros are running.";

            Logger.Trace($"Application exit cancelled: One or more macros are running.", new
            {
                InRunningProcessors = InRunningProcessors.Keys.Select(x => x.Macro.Key).ToArray(),
            });
        }
    }

    private void OnAppNotificationReceived(AppNotificationReceivedEvent e)
    {
        if (e.Arguments.TryGetValue("macroInstanceId", out var value))
        {
            var pageKey = GetPageKey(value);
            OpenPage(pageKey);
        }

        if (e.Arguments.ContainsKey(InteractionMessage.ProcessorIdKey))
        {
            var msg = new InteractionMessage(e.Arguments);
            SendMessage(msg);
        }
    }

}
