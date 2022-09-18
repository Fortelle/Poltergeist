using System;

namespace Poltergeist.Automations.Logging;

public struct LogEntry
{
    public string Name;
    public string Message;
    public LogLevel Level;
    public string Sender;
    public DateTime Timestamp;
}
