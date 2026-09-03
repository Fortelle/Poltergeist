using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros;

public class CommonConfiguralizationModule : MacroModule
{
    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);

        macro.OptionDefinitions.Add(new OptionDefinition<LogLevel>(MacroLogger.ToFileLevelKey, LogLevel.None)
        {
            Category = LocalizationUtil.Localize("MacroLoggerOption_Category"),
            DisplayLabel = LocalizationUtil.Localize("MacroLoggerOption_FileLogLevel"),
            IsGlobal = true,
        });

        macro.OptionDefinitions.Add(new OptionDefinition<LogLevel>(MacroLogger.ToDashboardLevelKey, LogLevel.Information)
        {
            Category = LocalizationUtil.Localize("MacroLoggerOption_Category"),
            DisplayLabel = LocalizationUtil.Localize("MacroLoggerOption_DashboardLogLevel"),
            IsGlobal = true,
        });

        macro.StatisticDefinitions.Add(new StatisticDefinition<int>("total_run_count", 0)
        {
            DisplayLabel = LocalizationUtil.Localize("Statistic_TotalRunCount"),
            Update = (total, _) => total + 1
        });

        macro.StatisticDefinitions.Add(new StatisticDefinition<TimeSpan>("total_run_duration")
        {
            DisplayLabel = LocalizationUtil.Localize("Statistic_TotalRunDuration"),
            TargetKey = "run_duration",
            Update = (total, next) => total + next,
            Format = x => $"{x.TotalHours:00}:{x.Minutes:00}:{x.Seconds:00}",
        });
    }
}
