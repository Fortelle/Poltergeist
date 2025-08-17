using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Modules.Macros;

public class MacroStatisticsService : ServiceBase
{
    public SavablePredefinedCollection GlobalStatistics { get; set; } = new();

    public MacroStatisticsService()
    {
        LoadGlobalStatistics();
    }

    public void UpdateStatistics(MacroInstance instance, ProcessorReport report)
    {
        ArgumentNullException.ThrowIfNull(instance.Template);

        foreach (var definition in instance.Template.StatisticDefinitions)
        {
            var statistics = definition.IsGlobal ? GlobalStatistics : instance.Statistics;
            if (statistics is null)
            {
                continue;
            }

            var oldValue = statistics.Get(definition.Key);
            if (definition.TryUpdate(oldValue, report, out var updatedValue) && !Equals(oldValue, updatedValue))
            {
                statistics.Set(definition.Key, updatedValue);
            }
        }

        instance.Statistics?.Save();
        SaveGlobalStatistics();
    }

    private void LoadGlobalStatistics()
    {
        var filepath = PoltergeistApplication.Paths.GlobalMacroStatisticsFile;

        try
        {
            GlobalStatistics.Load(filepath);

            Logger.Trace($"Loaded the global statistics.", new
            {
                Path = filepath,
            });
        }
        catch (FileNotFoundException)
        {
            Logger.Trace($"Skipped loading the global statistics: File does not exist.", new
            {
                Path = filepath,
            });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load the global statistics: {ex.Message}", new
            {
                Path = filepath,
                Exception = ex.GetType().Name,
                Message = ex.Message,
            });
            if (PoltergeistApplication.Current.IsDevelopment)
            {
                PoltergeistApplication.ShowTeachingTip($"Failed to load global statistics");
            }
        }
    }

    private void SaveGlobalStatistics()
    {
        try
        {
            if (GlobalStatistics.Save())
            {
                Logger.Trace("Saved the global statistics.", new
                {
                    GlobalStatistics.FilePath
                });
                if (PoltergeistApplication.Current.IsDevelopment)
                {
                    PoltergeistApplication.ShowTeachingTip($"Saved global statistics");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save the global statistics: {ex.Message}");
            if (PoltergeistApplication.Current.IsDevelopment)
            {
                PoltergeistApplication.ShowTeachingTip($"Failed to save global statistics");
            }
        }
    }
}
