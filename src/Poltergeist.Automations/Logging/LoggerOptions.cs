namespace Poltergeist.Automations.Logging;

public class LoggerOptions
{
    public string? Filename { get; set; }
    public LogLevel FileLogLevel { get; set; }
    public LogLevel FrontLogLevel { get; set; }
}
