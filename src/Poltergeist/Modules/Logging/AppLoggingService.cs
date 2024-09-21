using System.Collections.Concurrent;
using System.Text;

namespace Poltergeist.Modules.Logging;

public class AppLoggingService : IDisposable
{
    private const string FileNameFormat = "{0:yyyy-MM-dd_HH-mm-ss}.log";

    public readonly ConcurrentQueue<AppLogEntry> LogPool = new();
    public event Action<AppLogEntry>? Logged;

    private readonly bool IsTraceEnabled;
    private ConcurrentWriter? LogFileWriter;
    private bool IsDisposed;

    public AppLoggingService()
    {
#if DEBUG
        IsTraceEnabled = true;
        OpenFile(PoltergeistApplication.Paths.LogFolder);
#endif
    }

    private void OpenFile(string folder)
    {
        var fileName = string.Format(FileNameFormat, DateTime.Now);
        var filePath = Path.Combine(folder, fileName);
        try
        {
            LogFileWriter = new(filePath);
        }
        catch { }
    }

    public void Log(AppLogEntry entry)
    {
        if (entry.Level == AppLogLevel.Trace && !IsTraceEnabled)
        {
            return;
        }

        LogPool.Enqueue(entry);

        Logged?.Invoke(entry);

        ToFile(entry);
    }

    private void ToFile(AppLogEntry entry)
    {
        if (LogFileWriter is null)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.Append($"{entry.Timestamp:o}");
        sb.Append('\t');
        sb.Append($"{entry.Level}");
        sb.Append('\t');
        sb.Append($"{entry.Sender}");
        sb.Append('\t');
        sb.Append($"{entry.Message}");

        LogFileWriter.WriteLine(sb.ToString());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            LogFileWriter?.Dispose();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
