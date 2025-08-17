using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros;

public abstract class CommonMacroBase : MacroBase
{
    public CommonMacroBase() : this(null)
    {
    }

    public CommonMacroBase(string? name = null) : base(name)
    {
        OptionDefinitions.Add(new OptionDefinition<LogLevel>(MacroLogger.ToFileLevelKey, LogLevel.None)
        {
            Category = ResourceHelper.Localize($"Poltergeist.Automations/Resources/MacroLoggerOption_Category"),
            DisplayLabel = ResourceHelper.Localize($"Poltergeist.Automations/Resources/MacroLoggerOption_FileLogLevel"),
            IsGlobal = true,
        });

        OptionDefinitions.Add(new OptionDefinition<LogLevel>(MacroLogger.ToDashboardLevelKey, LogLevel.Information)
        {
            Category = ResourceHelper.Localize($"Poltergeist.Automations/Resources/MacroLoggerOption_Category"),
            DisplayLabel = ResourceHelper.Localize($"Poltergeist.Automations/Resources/MacroLoggerOption_DashboardLogLevel"),
            IsGlobal = true,
        });

        StatisticDefinitions.Add(new StatisticDefinition<int>("total_run_count", 0)
        {
            DisplayLabel = ResourceHelper.Localize($"Poltergeist.Automations/Resources/Statistic_TotalRunCount"),
            Update = (total, _) => total + 1
        });

        StatisticDefinitions.Add(new StatisticDefinition<TimeSpan>("total_run_duration")
        {
            DisplayLabel = ResourceHelper.Localize($"Poltergeist.Automations/Resources/Statistic_TotalRunDuration"),
            TargetKey = "run_duration",
            Update = (total, next) => total + next,
            Format = x => $"{x.TotalHours:00}:{x.Minutes:00}:{x.Seconds:00}",
        });
    }
}
