using Newtonsoft.Json;

namespace Poltergeist.Automations.Processors;

public sealed class ProcessHistoryEntry
{
    public required string MacroKey { get; init; }

    public required string ProcessId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required EndReason EndReason { get; init; }

    public string? Comment { get; init; }

    [JsonIgnore]
    public TimeSpan Duration => EndTime - StartTime;
}
