namespace Poltergeist.Modules.Logging;

public class AppLogEntry
{
    public required string Message { get; init; }
    public required AppLogLevel Level { get; init; }
    public required string Sender { get; init; }
    public required DateTime Timestamp { get; init; }

    public Dictionary<string, object>? Data { get; set; }

    public string this[string key]
    {
        set
        {
            Data ??= new();
            Data[key] = value;
        }
    }
}
