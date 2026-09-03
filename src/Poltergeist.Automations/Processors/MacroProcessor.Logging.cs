using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    private LoggerWrapper? Logger;

    private void Log(LogLevel level, string message)
    {
        if (Logger is not null)
        {
            Logger.Log(level, message);
            return;
        }
        else
        {
            var entry = new LogEntry()
            {
                Sender = nameof(MacroProcessor),
                Level = level,
                Message = message,
                Timestamp = DateTime.Now,
                ElapsedTime = GetElapsedTime(),
            };
            var args = new LogWrittenEventArgs(entry);
            RaiseEvent(ProcessorEvent.LogWritten, args);

            Console.WriteLine(message);
        }
    }
}
