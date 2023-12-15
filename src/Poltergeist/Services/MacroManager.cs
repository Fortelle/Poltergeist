using Newtonsoft.Json;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Services;

public class MacroManager
{
    public List<IMacroBase> Macros { get; } = new();
    public List<MacroGroup> Groups { get; } = new();
    public OptionCollection GlobalOptions { get; set; } = new();
    public ParameterCollection GlobalStatistics { get; set; } = new();

    public List<MacroProcessor> InRunningProcesses { get; set; } = new();

    public bool IsBusy => InRunningProcesses.Any();

    private readonly PathService PathService;
    private readonly LocalSettingsService LocalSettings;

    public Dictionary<string, MacroSummaryEntry> Summaries { get; } = new();

    public MacroManager(PathService path, LocalSettingsService localSettings)
    {
        PathService = path;
        LocalSettings = localSettings;

        App.ContentLoaded += () =>
        {
            LoadGlobalOptions();
            LoadGlobalStatistics();

            LoadSummary();
            var macroManager = App.GetService<MacroManager>();
            foreach(var summary in Summaries.Values)
            {
                var macro = macroManager.GetMacro(summary.MacroKey);
                if (macro is not null)
                {
                    summary.Title = macro.Title;
                    summary.IsAvailable = true;
                }
                else
                {
                    summary.IsAvailable = false;
                }
            }
        };
    }

    public void AddMacro(IMacroBase macro)
    {
        macro.PrivateFolder = PathService.GetMacroFolder(macro);
        macro.SharedFolder = PathService.SharedFolder;
        try
        {
            macro.Initialize();
        } catch (Exception)
        {
        }

        Macros.Add(macro);
    }

    public void AddGroup(MacroGroup group)
    {
        group.GroupFolder = PathService.GetGroupFolder(group);
        group.Load();

        Groups.Add(group);

        foreach (var macro in group.Macros)
        {
            macro.Group = group;
            AddMacro(macro);
        }
    }

    public void AddGroup<T>() where T : MacroGroup, new()
    {
        AddGroup(new T());
    }

    public IMacroBase? GetMacro(string key)
    {
        return Macros.FirstOrDefault(x => x.Key == key);
    }

    public void TryStart(MacroProcessor processor)
    {
        var macro = processor.Macro;

        if (InRunningProcesses.Contains(processor))
        {
            throw new Exception("The macro is already running.");
        }

        InRunningProcesses.Add(processor);

        processor.Completed += Processor_Completed;

        processor.Launch();

        UpdateSummary(processor.Macro.Key, x =>
        {
            x.LastRunTime = processor.Statistics.Get<DateTime>("last_run_time");
            x.RunCount = processor.Statistics.Get<int>("total_run_count");
        });
    }

    private void Processor_Completed(object? sender, Automations.Processors.Events.MacroCompletedEventArgs e)
    {
        var processor = (MacroProcessor)sender!;

        processor.Completed -= Processor_Completed;

        var macro = processor.Macro;
        InRunningProcesses.Remove(processor);

        if (processor.Environments.Get(MacroBase.UseStatisticsKey, true))
        {
            var globalStatistics = processor.Statistics.ToValueDictionary(ParameterSource.Global);
            if (globalStatistics.Any())
            {
                foreach (var (key, value) in globalStatistics)
                {
                    GlobalStatistics.Update(key, value);
                }
                GlobalStatistics.Save();
            }
        }
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
        var processor = InRunningProcesses.FirstOrDefault(x => x.ProcessId == message.ProcessId);
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

    private void LoadSummary()
    {
        Summaries.Clear();

        var summaryPath = PathService.SummariesFile;
        if (!File.Exists(summaryPath))
        {
            return;
        }

        try
        {
            SerializationUtil.JsonLoad<MacroSummaryEntry[]>(summaryPath, out var list);
            foreach(var summary in list)
            {
                Summaries.Add(summary.MacroKey, summary);
            }
        }
        catch
        {

        }
    }

    public MacroSummaryEntry? GetSummary(string macroKey)
    {
        if (Summaries.TryGetValue(macroKey, out var entry))
        {
            return entry;
        }

        return null;
    }

    public void UpdateSummary(string key, Action<MacroSummaryEntry>? action = null)
    {
        if(!Summaries.TryGetValue(key, out var entry))
        {
            var macro = GetMacro(key)!;
            entry = new MacroSummaryEntry()
            {
                MacroKey = key,
                Title = macro.Title,
                IsAvailable = macro.IsAvailable,
            };
            Summaries.Add(key, entry);
        }

        action?.Invoke(entry);

        var summaryPath = PathService.SummariesFile;
        SerializationUtil.JsonSave(summaryPath, Summaries.Values);
    }

    public MacroProcessor CreateProcessor(IMacroBase macro, LaunchReason reason)
    {
        var processor = new MacroProcessor(macro, reason);

        PushEnvironments(processor.Environments);
        PushGlobalOptions(processor.Options);
        PushGlobalStatistics(processor.Statistics);

        return processor;
    }

    public void PushEnvironments(VariableCollection vc)
    {
        vc.Add("application_name", PathService.ApplicationName, ParameterSource.Global);

        var localSettings = App.GetService<LocalSettingsService>();
        var dict = localSettings.Settings.ToDictionary();

        vc.AddRange(dict, ParameterSource.Global);

        var pathService = App.GetService<PathService>();
        vc.Add("document_data_folder", pathService.DocumentDataFolder, ParameterSource.Global);
        vc.Add("shared_folder", pathService.SharedFolder, ParameterSource.Global);
        vc.Add("macro_folder", pathService.MacroFolder, ParameterSource.Global);
        vc.Add("group_folder", pathService.GroupFolder, ParameterSource.Global);
    }

    public void PushGlobalOptions(VariableCollection vc)
    {
        var globalOptions = GlobalOptions.ToDictionary();
        foreach (var (key, value) in globalOptions)
        {
            if (vc.Contains(key))
            {
                continue;
            }
            vc.Add(key, value, ParameterSource.Global);
        }
    }

    private void PushGlobalStatistics(VariableCollection vc)
    {
        var globalStatistics = GlobalOptions.ToDictionary();
        foreach (var (key, value) in globalStatistics)
        {
            if (vc.Contains(key))
            {
                continue;
            }
            vc.Add(key, value, ParameterSource.Global);
        }
    }

}

public class MacroSummaryEntry
{
    public required string MacroKey { get; set; }
    public required string Title { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime LastRunTime { get; set; }
    public int RunCount { get; set; }

    [JsonIgnore]
    public bool IsAvailable { get; set; }
}
