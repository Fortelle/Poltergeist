using Newtonsoft.Json;

namespace Poltergeist.Automations.Processors;

// use ProcessSummaryCreatedHook to push comment and extra message

public sealed class ProcessHistoryEntry
{
    public required string MacroKey { get; init; }

    public required string ProcessId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required EndReason EndReason { get; init; }

    public string? Comment { get; init; }

    public Dictionary<string, string>? Extra { get; set; }

    public ProcessHistoryEntry()
    {
    }

    public void Add(string key, string value)
    {
        Extra ??= new();
        Extra.Add(key, value);
    }

    [JsonIgnore]
    public TimeSpan Duration => EndTime - StartTime;
}
