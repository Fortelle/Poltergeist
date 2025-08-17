namespace Poltergeist.Automations.Components.Logging;

public class LogWrittenEventArgs : EventArgs
{
    public LogEntry Entry { get; set; }
    
    public LogWrittenEventArgs(LogEntry entry)
    {
        Entry = entry;
    }
}
