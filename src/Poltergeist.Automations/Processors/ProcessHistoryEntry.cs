namespace Poltergeist.Automations.Processors;

public sealed class ProcessHistoryEntry
{
    public required string MacroKey { get; init; }

    public required string ProcessId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required EndReason EndReason { get; init; }

    public string? Comment { get; init; }

    private readonly TimeSpan? _duration;
    public TimeSpan Duration
    {
        get => _duration ?? EndTime - StartTime;
        init => _duration = value;
    }
}
