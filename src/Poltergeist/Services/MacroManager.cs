using Newtonsoft.Json;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Services;

public class MacroManager
{
    private const string MacroSummariesKey = "app.macrosummaries";

    public List<IMacroBase> Macros { get; } = new();
    public List<MacroGroup> Groups { get; } = new();
    public MacroOptions GlobalOptions { get; set; } = new();

    public List<MacroProcessor> InRunningProcesses { get; set; } = new();

    public bool IsBusy => InRunningProcesses.Any();

    private readonly PathService PathService;
    private readonly LocalSettingsService LocalSettings;

    public Dictionary<string, MacroSummaryEntry> Summaries { get; } = new();

    public MacroManager(PathService path, LocalSettingsService localSettings)
    {
        PathService = path;
        LocalSettings = localSettings;

        App.SettingsLoading += (settings) =>
        {
            settings.Add(new OptionItem<MacroSummaryEntry[]>(MacroSummariesKey, Array.Empty<MacroSummaryEntry>())
            {
                IsBrowsable = false,
            });
        };

        App.ContentLoading += () =>
        {
            var summaries = LocalSettings.Get<MacroSummaryEntry[]>(MacroSummariesKey);
            if(summaries?.Length > 0)
            {
                foreach (var summary in summaries)
                {
                    Summaries.Add(summary.MacroKey, summary);
                }
            }
        };

        App.ContentLoaded += () =>
        {
            LoadGlobalOptions();

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
        group.LoadOptions();

        Groups.Add(group);

        foreach (var macro in group.Macros)
        {
            macro.Group = group;
            AddMacro(macro);
        }
    }

    public IMacroBase? GetMacro(string name)
    {
        return Macros.FirstOrDefault(x => x.Name == name);
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

        UpdateSummary(processor.Macro.Name, x =>
        {
            x.LastRunTime = processor.GetStatistic<DateTime>("last_run_time");
            x.RunCount = processor.GetStatistic<int>("total_run_count");
        });
    }

    private void Processor_Completed(object? sender, Automations.Processors.Events.MacroCompletedEventArgs e)
    {
        var processor = (MacroProcessor)sender!;

        processor.Completed -= Processor_Completed;

        var macro = processor.Macro;
        InRunningProcesses.Remove(processor);
    }

    public void AddGlobalOption(IOptionItem option)
    {
        GlobalOptions.Add(option);
    }

    public void LoadGlobalOptions()
    {
        foreach(var group in Groups)
        {
            group.SetGlobalOptions(GlobalOptions);
        }

        foreach (var module in Macros.SelectMany(x=>x.Modules))
        {
            module.SetGlobalOptions(GlobalOptions);
        }

        var filepath = PathService.GlobalMacroOptionsFile;
        GlobalOptions.Load(filepath);
    }

    public void SaveGlobalOptions()
    {
        var filepath = PathService.GlobalMacroOptionsFile;
        GlobalOptions.Save(filepath);
    }

    public void SendMessage(InteractionMessage message)
    {
        var processor = InRunningProcesses.FirstOrDefault(x => x.ProcessId == message.ProcessId);
        if (processor == null)
        {
            return;
        }
        if (message.MacroKey != processor.Macro.Name)
        {
            return;
        }

        processor.ReceiveMessage(message.ToDictionary());
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

        LocalSettings.Set(MacroSummariesKey, Summaries.Values.ToArray());
        LocalSettings.Save();
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