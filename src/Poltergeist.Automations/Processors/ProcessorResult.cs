using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public class ProcessorResult
{
    public required IReadOnlyParameterValueCollection Report { get; init; }

    public required IReadOnlyParameterValueCollection Outputs { get; init; }

    public Exception? Exception { get; init; }

    public string? MacroKey => Report.GetValueOrDefault(MacroKeyDefinition);
    public string? ProcessorId => Report.GetValueOrDefault(ProcessorIdDefinition);
    public DateTime StartTime => Report.GetValueOrDefault(StartTimeDefinition);
    public DateTime EndTime => Report.GetValueOrDefault(EndTimeDefinition);
    public TimeSpan Duration => Report.GetValueOrDefault(DurationDefinition);
    public string? Comment => Report.GetValueOrDefault(CommentDefinition);
    public ProcessorConclusion Conclusion => Report.GetValueOrDefault(ConclusionDefinition);

    public bool IsSuccess => Conclusion == ProcessorConclusion.Success;

    public static readonly EntryDefinition<string> MacroKeyDefinition = new("macro_key");
    public static readonly EntryDefinition<string> ProcessorIdDefinition = new("processor_id");
    public static readonly EntryDefinition<DateTime> StartTimeDefinition = new("start_time");
    public static readonly EntryDefinition<DateTime> EndTimeDefinition = new("end_time");
    public static readonly EntryDefinition<TimeSpan> DurationDefinition = new("run_duration");
    public static readonly EntryDefinition<string> CommentDefinition = new("comment_message");
    public static readonly EntryDefinition<ProcessorConclusion> ConclusionDefinition = new("conclusion");
}
