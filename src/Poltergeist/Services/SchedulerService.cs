using System.Collections.Generic;

using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Services;

public class SchedulerService : ObservableRecipient
{
    public List<MacroSchedule> Schedules;

    public SchedulerService(PathService pathService)
    {
        var filepath = pathService.LocalSettingsFile;
        SerializationUtil.JsonLoad(filepath, out Schedules);
        Schedules ??= new();
    }

}

public class MacroSchedule
{
    public bool Enable { get; set; }
    public string Cron { get; set; } = "* * * * *";
    public string MacroKey { get; set; }

    public ScheduleStatus Status { get; set; }
}

public enum ScheduleStatus
{
    Disabled,
    Outdated,
    Completed,

}
