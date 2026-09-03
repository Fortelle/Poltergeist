using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Pages.Macros;

public class ProcessorHistoryEntry
{
    public string? MacroKey { get; set; }

    public string? ProcessorId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public TimeSpan Duration { get; set; }

    public ProcessorConclusion Conclusion { get; set; }

    public string? Comment { get; set; }

    public static ProcessorHistoryEntry FromReport(ProcessorReport report)
    {
        return new ProcessorHistoryEntry()
        {
            MacroKey = report.GetValueOrDefault(ProcessorResult.MacroKeyDefinition),
            ProcessorId = report.GetValueOrDefault(ProcessorResult.ProcessorIdDefinition),
            StartTime = report.GetValueOrDefault(ProcessorResult.StartTimeDefinition),
            EndTime = report.GetValueOrDefault(ProcessorResult.EndTimeDefinition),
            Duration = report.GetValueOrDefault(ProcessorResult.DurationDefinition),
            Conclusion = report.GetValueOrDefault(ProcessorResult.ConclusionDefinition),
            Comment = report.GetValueOrDefault(ProcessorResult.CommentDefinition),
        };
    }
}
