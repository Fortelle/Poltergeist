using System.Reflection;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Cryptology;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Settings;
using Poltergeist.UI.Pages.Macros;

namespace Poltergeist.Modules.Macros;

public class MacroManager : ServiceBase
{
    public List<MacroShell> Shells { get; } = new();
    public List<IFrontMacro> Templates { get; } = new();
    public ParameterDefinitionValueCollection GlobalOptions { get; set; } = new();
    public ParameterDefinitionValueCollection GlobalStatistics { get; set; } = new();

    public List<IFrontProcessor> InRunningProcessors { get; set; } = new();

    public MacroManager(AppEventService eventService)
    {
        eventService.Subscribe<AppContentLoadedHandler>(OnAppContentLoaded);
        eventService.Subscribe<AppWindowClosingHandler>(OnAppWindowClosing);
        eventService.Subscribe<AppWindowClosedHandler>(OnAppWindowClosed);
    }

    public void AddMacro(IFrontMacro macro)
    {
        macro.Initialize();

        var shells = Shells.Where(x => x.Properties.TemplateKey == macro.Key).ToArray();
        foreach (var shell in shells)
        {
            shell.Template ??= macro;
        }

        if (macro.IsSingleton)
        {
            if (shells.Length == 0)
            {
                AddMacro(new MacroShell(macro));
            }
        }
        else
        {
            if (Templates.Any(x => x.Key == macro.Key))
            {
                Logger.Warn($"Skipped adding macro template '{macro.Key}': A macro template with the same name already exists.");
            }
            else
            {
                Templates.Add(macro);
                Logger.Trace($"Added macro template '{macro.Key}'.");
            }
        }
    }

    public void AddMacro(MacroShell shell)
    {
        shell.PrivateFolder = Path.Combine(PoltergeistApplication.Paths.MacroFolder, shell.ShellKey);
        shell.Template?.Initialize();
        Shells.Add(shell);

        Logger.Trace($"Added macro instance '{shell.ShellKey}'.");

    }

    public void NewMacro(MacroShell shell)
    {
        AddMacro(shell);
        SaveProperties();

        PoltergeistApplication.GetService<AppEventService>().Raise(new MacroCollectionChangedHandler()
        {
            NewItems = [shell],
        });

        Logger.Info($"Created new macro '{shell.ShellKey}'.");
    }

    public bool RemoveMacro(MacroShell shell)
    {
        Logger.Trace($"Deleting macro '{shell.ShellKey}'.");

        if (!Shells.Contains(shell))
        {
            return false;
        }

        Shells.Remove(shell);

        if (shell.PrivateFolder is not null && Directory.Exists(shell.PrivateFolder))
        {
            try
            {
                FileSystem.DeleteDirectory(shell.PrivateFolder, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                Logger.Trace($"Deleted folder '{shell.PrivateFolder}'.");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to delete folder '{shell.PrivateFolder}': {ex.Message}");
            }
        }

        PoltergeistApplication.GetService<AppEventService>().Raise(new MacroCollectionChangedHandler()
        {
            OldItems = [shell],
        });

        SaveProperties();

        Logger.Info($"Deleted macro '{shell.ShellKey}'.");

        return true;
    }

    public IEnumerable<MacroBase> LoadMacrosFromGroup(MacroGroup group)
    {
        var groupType = group.GetType();

        try
        {
            group.Load();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load macro group '{groupType.Name}': {ex.Message}.");
        }

        var fields = groupType
            .GetFields()
            .Where(field => field.GetCustomAttribute<AutoLoadAttribute>() is not null)
            .Where(field => field.FieldType.IsAssignableTo(typeof(MacroBase)))
            ;
        foreach (var field in fields)
        {
            yield return (MacroBase)field.GetValue(group)!;
        }

        var properties = groupType
            .GetProperties()
            .Where(property => property.GetCustomAttribute<AutoLoadAttribute>() is not null)
            .Where(property => property.PropertyType.IsAssignableTo(typeof(MacroBase)))
            ;
        foreach (var property in properties)
        {
            yield return (MacroBase)property.GetValue(group)!;
        }

        var methods = groupType
            .GetMethods()
            .Where(method => method.GetCustomAttribute<AutoLoadAttribute>() is not null)
            .Where(method => method.ReturnType.IsAssignableTo(typeof(MacroBase)))
            ;
        foreach (var method in methods)
        {
            MacroBase? macro = null;
            try
            {
                macro = (MacroBase)method.Invoke(group, null)!;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to create an instance of macro '{method.ReturnType.Name}': {ex.Message}.");
                continue;
            }
            yield return macro;
        }

        var types = groupType
            .GetNestedTypes()
            .Where(type => type.IsAssignableTo(typeof(MacroBase)))
            .Where(type => type.GetCustomAttribute<AutoLoadAttribute>() is not null)
            ;
        foreach (var type in types)
        {
            MacroBase? macro = null;
            try
            {
                macro = (MacroBase)Activator.CreateInstance(type)!;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to create an instance of macro '{type.Name}': {ex.Message}");
                continue;
            }
            yield return macro;
        }
    }

    public IFrontMacro? GetTemplate(string macroKey)
    {
        return Templates.FirstOrDefault(x => x.Key == macroKey);
    }

    public MacroShell? GetShell(string shellKey)
    {
        return Shells.FirstOrDefault(x => x.ShellKey == shellKey);
    }

    public MacroProcessor CreateProcessor(MacroStartArguments args)
    {
        var shell = GetShell(args.ShellKey);

        if (shell is null)
        {
            throw new KeyNotFoundException();
        }

        if (shell.Template is null)
        {
            throw new InvalidOperationException();
        }

        var options = new Dictionary<string, object?>();
        foreach (var (definition, value) in GlobalOptions.GetDefinitionValueCollection())
        {
            options[definition.Key] = value;
        }
        if (!args.IgnoresUserOptions && shell.UserOptions is not null)
        {
            foreach (var (definition, value) in shell.UserOptions.GetDefinitionValueCollection())
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

        var statistics = new Dictionary<string, object?>();
        foreach (var (definition, value) in GlobalStatistics.GetDefinitionValueCollection())
        {
            statistics[definition.Key] = value;
        }
        if (shell.Statistics is not null)
        {
            foreach (var (definition, value) in shell.Statistics.GetDefinitionValueCollection())
            {
                statistics[definition.Key] = value;
            }
        }

        var environments = GetEnvironments(shell);
        if (args.EnvironmentOverrides is not null)
        {
            foreach (var (key, value) in args.EnvironmentOverrides)
            {
                environments[key] = value;
            }
        }

        var processorArguments = new MacroProcessorArguments
        {
            LaunchReason = args.Reason,
            Options = options,
            Statistics = statistics,
            Environments = environments,
            SessionStorage = args.SessionStorage,
        };

        var processor = new MacroProcessor((MacroBase)shell.Template, processorArguments);

        return processor;
    }

    public Dictionary<string, object?> GetOptions(MacroShell shell)
    {
        var options = new Dictionary<string, object?>();
        foreach (var (definition, value) in GlobalOptions.GetDefinitionValueCollection())
        {
            options[definition.Key] = value;
        }
        if (shell.UserOptions is not null)
        {
            foreach (var (definition, value) in shell.UserOptions.GetDefinitionValueCollection())
            {
                options[definition.Key] = value;
            }
        }
        return options;
    }

    public Dictionary<string, object?> GetEnvironments(MacroShell shell)
    {
        var environments = new Dictionary<string, object?>
        {
            { "application_name", PoltergeistApplication.ApplicationName },
            { "document_data_folder", PoltergeistApplication.Paths.DocumentDataFolder },
            { "shared_folder", PoltergeistApplication.Paths.SharedFolder },
            { "macro_folder", PoltergeistApplication.Paths.MacroFolder },
            { "is_development", PoltergeistApplication.IsDevelopment },
            { "is_administrator", PoltergeistApplication.IsAdministrator },
            { "is_singlemode", PoltergeistApplication.SingleMacroMode is not null },
            { "shell_key", shell.ShellKey },
        };

        var settings = PoltergeistApplication.GetService<AppSettingsService>();
        foreach (var (definition, value) in settings.Settings.GetDefinitionValueCollection())
        {
            environments.Add(definition.Key, value);
        }

        if (!string.IsNullOrEmpty(shell.PrivateFolder))
        {
            environments.Add("private_folder", shell.PrivateFolder);
        }

        return environments;
    }

    public void Launch(IFrontProcessor processor)
    {
        Logger.Trace($"Launching macro processor.", new
        {
            TemplateKey = processor.Macro.Key,
            processor.ProcessId,
            processor.Options,
            processor.Statistics,
            processor.Environments,
            SessionStorage = processor.SessionStorage.Keys.ToArray(),
        });

        if (InRunningProcessors.Contains(processor))
        {
            throw new InvalidOperationException("The macro is already running.");
        }

        InRunningProcessors.Add(processor);

        processor.Launched += Processor_Launched;
        processor.Completed += Processor_Completed;

        Logger.Info($"Macro '{processor.Macro.Key}' started.");

        processor.Run();
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
                var shellKey = processor.Environments.GetValueOrDefault<string>("shell_key");
            if (shellKey is not null)
            {
                var shell = GetShell(shellKey)!;
                UpdateProperties(shell, properties =>
                {
                    properties.LastRunTime = processor.Statistics.GetValueOrDefault<DateTime>("last_run_time");
                    properties.RunCount = processor.Statistics.GetValueOrDefault<int>("total_run_count");
                });
            }
        }

        PoltergeistApplication.GetService<AppEventService>().Raise(new MacroRunningHandler(processor));
    }

    private void Processor_Completed(object? sender, ProcessorCompletedEventArgs e)
    {
        if (sender is not IFrontProcessor processor)
        {
            throw new InvalidOperationException();
        }

        processor.Completed -= Processor_Completed;

        InRunningProcessors.Remove(processor);

        var shellKey = processor.Environments.GetValueOrDefault<string>("shell_key");

        if (shellKey is null)
        {
            return;
        }

        var shell = GetShell(shellKey)!;

        if (!processor.IsIncognitoMode())
        {
            foreach (var (key, value) in processor.Statistics)
            {
                if (shell.Statistics?.ContainsDefinition(key) == true)
                {
                    shell.Statistics.Set(key, value);
                }
                else if (GlobalStatistics.ContainsDefinition(key) == true)
                {
                    GlobalStatistics.Set(key, value);
                }
            }

            try
            {
                shell.Statistics?.Save();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save shell statistics: {ex.Message}");
            }

            SaveGlobaStatistics();

            if (shell.History is not null && e.HistoryEntry is not null)
            {
                shell.History.Add(e.HistoryEntry);
                try
                {
                    shell.History.Save();
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to save shell history: {ex.Message}");
                }
            }
        }

        PoltergeistApplication.GetService<AppEventService>().Raise<MacroCompletedHandler>(new(shell));
       
        Logger.Info($"Macro '{processor.Macro.Key}' ended.");
    }

    private void LoadGlobalOptions()
    {
        foreach (var item in MacroModule.GlobalOptions)
        {
            GlobalOptions.AddDefinition(item);
        }

        var filepath = PoltergeistApplication.Paths.GlobalMacroOptionsFile;

        try
        {
            GlobalOptions.Load(filepath);

            Logger.Trace($"Loaded global options.", new
            {
                Path = filepath,
            });
        }
        catch (FileNotFoundException)
        {
            Logger.Trace($"Skipped loading global options: File does not exist.", new
            {
                Path = filepath,
            });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load global options: {ex.Message}", new
            {
                Path = filepath,
                Exception = ex.GetType().Name,
                Message = ex.Message,
            });
        }
    }

    private void LoadGlobalStatistics()
    {
        foreach (var item in MacroModule.GlobalStatistics)
        {
            GlobalStatistics.AddDefinition(item);
        }

        var filepath = PoltergeistApplication.Paths.GlobalMacroStatisticsFile;

        try
        {
            GlobalStatistics.Load(filepath);

            Logger.Trace($"Loaded global statistics.", new
            {
                Path = filepath,
            });
        }
        catch (FileNotFoundException)
        {
            Logger.Trace($"Skipped loading global statistics: File does not exist.", new
            {
                Path = filepath,
            });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load global statistics: {ex.Message}", new
            {
                Path = filepath,
                Exception = ex.GetType().Name,
                Message = ex.Message,
            });
        }
    }

    private void SaveGlobalOptions()
    {
        try
        {
            GlobalOptions.Save();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save global options: {ex.Message}");
        }
    }

    private void SaveGlobaStatistics()
    {
        try
        {
            GlobalStatistics.Save();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save global statistics: {ex.Message}");
        }
    }

    public void SendMessage(InteractionMessage message)
    {
        var processor = InRunningProcessors.FirstOrDefault(x => x.ProcessId == message.ProcessId);
        if (processor is null)
        {
            Logger.Warn($"Failed to send message: Processor not found.", new
            {
                message.MacroKey,
                message.ProcessId,
            });
            return;
        }
        if (message.MacroKey != processor.Macro.Key)
        {
            Logger.Warn($"Failed to send message: Wrong macro key.", new
            {
                message.MacroKey,
                message.ProcessId,
            });
            return;
        }

        Logger.Trace($"Sending message to processor.", new
        {
            message.MacroKey,
            message.ProcessId,
        });

        processor.ReceiveMessage(message.ToDictionary());
    }

    public void LoadProperties()
    {
        var propertiesPath = PoltergeistApplication.Paths.MacroPropertiesFile;
        if (!File.Exists(propertiesPath))
        {
            Logger.Trace($"Skipped loading macro properties: File does not exist.", new
            {
                Path = propertiesPath,
            });
            return;
        }

        try
        {
            SerializationUtil.JsonLoad<MacroProperties[]>(propertiesPath, out var propertiesList);
            foreach (var properties in propertiesList!)
            {
                var template = GetTemplate(properties.TemplateKey);
                if (template is not null)
                {
                    var shell = new MacroShell(template, properties);
                    AddMacro(shell);
                }
                else
                {
                    var shell = GetShell(properties.ShellKey);
                    if (shell is not null)
                    {
                        shell.Properties = properties;
                    }
                    else
                    {
                        AddMacro(new MacroShell(properties));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Trace($"Failed to load macro properties: {ex.Message}", new
            {
                Path = propertiesPath,
            });
        }
    }

    public MacroProperties? GetProperties(string shellKey)
    {
        var shell = GetShell(shellKey);
        return shell?.Properties;
    }

    public void UpdateProperties(MacroShell shell, Action<MacroProperties> action)
    {
        action.Invoke(shell.Properties);

        PoltergeistApplication.GetService<AppEventService>().Raise(new MacroPropertyChangedHandler(shell));

        SaveProperties();
    }

    public void SaveProperties()
    {
        var propertiesPath = PoltergeistApplication.Paths.MacroPropertiesFile;
        var serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
        var propertiesList = Shells
            .Select(x => x.Properties)
            .Select(x => JObject.FromObject(x, serializer))
            .Where(x => x.Values().Count() > 2)
            .ToArray();
        try
        {
            SerializationUtil.JsonSave(propertiesPath, propertiesList);
            Logger.Trace($"Saved macro property file '{propertiesPath}'.");
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save macro property file '{propertiesPath}': {ex.Message}");
        }
    }

    private void OnAppContentLoaded(AppContentLoadedHandler e)
    {
        LoadGlobalOptions();
        LoadGlobalStatistics();
        LoadProperties();
    }

    private void OnAppWindowClosed(AppWindowClosedHandler e)
    {
        SaveGlobalOptions();
    }

    private void OnAppWindowClosing(AppWindowClosingHandler e)
    {
        if (InRunningProcessors.Count > 0)
        {
            e.Cancel = true;
            e.CancelMessage = "One or more macros are running.";

            Logger.Trace($"Application exit cancelled: One or more macros are running.", new
            {
                InRunningProcessors = InRunningProcessors.Select(x => x.Macro.Key).ToArray(),
            });
        }
    }
}
