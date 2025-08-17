using Poltergeist.Automations.Processors;

namespace Poltergeist.UI.Pages.Macros;

public class ProcessorHistoryEntry
{
    public string? MacroKey { get; set; }

    public string? ProcessorId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public TimeSpan Duration { get; set; }

    public EndReason EndReason { get; set; }

    public string? Comment { get; set; }
}
