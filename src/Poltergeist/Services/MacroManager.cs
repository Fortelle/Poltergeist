using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Cryptology;
using Poltergeist.Pages.Macros;

namespace Poltergeist.Services;

public class MacroManager
{
    public List<IFrontMacro> Templates { get; } = new();

    public List<MacroShell> Shells { get; } = new();

    public ParameterDefinitionValueCollection GlobalOptions { get; set; } = new();
    public ParameterDefinitionValueCollection GlobalStatistics { get; set; } = new();

    public List<IFrontProcessor> InRunningProcessors { get; set; } = new();

    public bool IsBusy => InRunningProcessors.Any();

    private readonly PathService PathService;

    public event Action? MacroCollectionChanged;
    public event Action? MacroPropertyChanged;
    public event Action<MacroShell>? MacroProcessorCompleted;

    public MacroManager(PathService path)
    {
        PathService = path;

        LoadGlobalOptions();
        LoadGlobalStatistics();
    }

    public void AddMacro(IFrontMacro macro)
    {
        macro.Initialize();

        if (macro.IsSingleton)
        {
            var shell = new MacroShell(macro);
            AddMacro(shell);
        }
        else
        {
            Templates.Add(macro);
        }
    }

    public void AddMacro(MacroShell shell)
    {
        shell.PrivateFolder = Path.Combine(PathService.MacroFolder, shell.ShellKey);
        shell.Template?.Initialize();
        Shells.Add(shell);

        MacroCollectionChanged?.Invoke();
    }

    public void RemoveMacro(MacroShell shell)
    {
        if (!Shells.Contains(shell))
        {
            return;
        }

        Shells.Remove(shell);

        if (shell.PrivateFolder is not null && Directory.Exists(shell.PrivateFolder))
        {
            try
            {
                FileSystem.DeleteDirectory(shell.PrivateFolder, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch {}
        }

        MacroCollectionChanged?.Invoke();

        SaveProperties();
    }

    public void LoadGroup(MacroGroup group)
    {
        group.Load();

        foreach (var macro in group.ReadMacroFields())
        {
            AddMacro(macro);
        }
        foreach (var macro in group.ReadMacroFunctions())
        {
            AddMacro(macro);
        }
        foreach (var macro in group.ReadMacroClasses())
        {
            AddMacro(macro);
        }
    }

    public void LoadGroup<T>() where T : MacroGroup, new()
    {
        LoadGroup(new T());
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

        var processor = new MacroProcessor(shell.Template, args.Reason, args.ShellKey);

        if (processor.Exception is not null)
        {
            return processor;
        }

        if (!args.IgnoresUserOptions)
        {
            foreach (var (key, value) in GetOptions(shell))
            {
                processor.Options.Reset(key, value);
            }
        }
        if (args.OptionOverrides is not null)
        {
            foreach (var (key, value) in args.OptionOverrides)
            {
                processor.Options.Reset(key, value);
            }
        }

        foreach (var (definition, value) in GlobalStatistics.ToDefinitionValueArray())
        {
            processor.Statistics.Reset(definition.Key, value);
        }
        if (shell.Statistics is not null)
        {
            foreach (var (definition, value) in shell.Statistics.ToDefinitionValueArray())
            {
                processor.Statistics.Reset(definition.Key, value);
            }
        }

        foreach (var (key, value) in GetEnvironments(shell))
        {
            processor.Environments.Reset(key, value);
        }
        if (args.EnvironmentOverrides is not null)
        {
            foreach (var (key, value) in args.EnvironmentOverrides)
            {
                processor.Environments.Reset(key, value);
            }
        }

        if (args.SessionStorage is not null)
        {
            foreach (var (key, value) in args.SessionStorage)
            {
                processor.SessionStorage.Reset(key, value);
            }
        }

        return processor;
    }

    public Dictionary<string, object?> GetOptions(MacroShell shell)
    {
        var options = new Dictionary<string, object?>();
        foreach (var (definition, value) in GlobalOptions.ToDefinitionValueArray())
        {
            options[definition.Key] = value;
        }
        if (shell.UserOptions is not null)
        {
            foreach (var (definition, value) in shell.UserOptions.ToDefinitionValueArray())
            {
                options[definition.Key] = value;
            }
        }
        return options;
    }

    public Dictionary<string, object?> GetEnvironments(MacroShell shell)
    {
        var environments = new Dictionary<string, object?>();

        var pathService = App.GetService<PathService>();
        environments.Add("application_name", PathService.ApplicationName);
        environments.Add("document_data_folder", pathService.DocumentDataFolder);
        environments.Add("shared_folder", pathService.SharedFolder);
        environments.Add("macro_folder", pathService.MacroFolder);
        environments.Add("is_development", App.IsDevelopment);

        var localSettings = App.GetService<LocalSettingsService>();
        foreach (var (definition, value) in localSettings.Settings.ToDefinitionValueArray())
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
        var macro = processor.Macro;

        if (InRunningProcessors.Contains(processor))
        {
            throw new Exception("The macro is already running.");
        }

        InRunningProcessors.Add(processor);

        processor.Completed += Processor_Completed;

        processor.Launch();

        UpdateProperties(processor.ShellKey!, properties =>
        {
            properties.LastRunTime = processor.Statistics.Get<DateTime>("last_run_time");
            properties.RunCount = processor.Statistics.Get<int>("total_run_count");
        });
    }

    private void Processor_Completed(object? sender, ProcessorCompletedEventArgs e)
    {
        var processor = (IFrontProcessor)sender!;

        processor.Completed -= Processor_Completed;

        InRunningProcessors.Remove(processor);

        if (processor.ShellKey is null)
        {
            return;
        }
        var shell = GetShell(processor.ShellKey)!;

        if (processor.Environments.Get(MacroBase.UseStatisticsKey, true))
        {
            foreach (var entry in processor.Statistics)
            {
                if (shell.Statistics?.ContainsKey(entry.Key) == true)
                {
                    shell.Statistics.Set(entry.Key, entry.Value);
                }
                else if (GlobalStatistics.ContainsKey(entry.Key) == true)
                {
                    GlobalStatistics.Set(entry.Key, entry.Value);
                }
            }

            shell.Statistics?.Save();
            GlobalStatistics.Save();
        }

        shell.History?.Add(e.HistoryEntry);

        MacroProcessorCompleted?.Invoke(shell);
    }

    private void LoadGlobalOptions()
    {
        foreach (var item in MacroModule.GlobalOptions)
        {
            GlobalOptions.Add(item);
        }

        var filepath = PathService.GlobalMacroOptionsFile;
        GlobalOptions.Load(filepath);
    }

    private void LoadGlobalStatistics()
    {
        foreach (var item in MacroModule.GlobalStatistics)
        {
            GlobalStatistics.Add(item);
        }

        var filepath = PathService.GlobalMacroStatisticsFile;
        GlobalStatistics.Load(filepath);
    }

    public void SendMessage(InteractionMessage message)
    {
        var processor = InRunningProcessors.FirstOrDefault(x => x.ProcessId == message.ProcessId);
        if (processor == null)
        {
            return;
        }
        if (message.MacroKey != processor.Macro.Key)
        {
            return;
        }

        processor.ReceiveMessage(message.ToDictionary());
    }

    public void LoadProperties()
    {
        var propertiesPath = PathService.MacroPropertiesFile;
        if (!File.Exists(propertiesPath))
        {
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
        catch
        {
        }
    }

    public MacroProperties? GetProperties(string shellKey)
    {
        var shell = GetShell(shellKey);
        return shell?.Properties;
    }

    public void UpdateProperties(string shellKey, Action<MacroProperties> action)
    {
        var shell = GetShell(shellKey);
        if (shell is null)
        {
            throw new KeyNotFoundException();
        }

        action.Invoke(shell.Properties);

        MacroPropertyChanged?.Invoke();

        SaveProperties();
    }

    public void SaveProperties()
    {
        var propertiesPath = PathService.MacroPropertiesFile;
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
        SerializationUtil.JsonSave(propertiesPath, propertiesList);
    }
}
