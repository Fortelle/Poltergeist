namespace Poltergeist.Automations.Components.Logging;

public class LogEntry
{
    public required string Message { get; init; }
    public required LogLevel Level { get; init; }
    public required string Sender { get; init; }
    public required DateTime Timestamp { get; init; }
    public required TimeSpan ElapsedTime { get; init; }
    public int IndentLevel { get; init; }
}
