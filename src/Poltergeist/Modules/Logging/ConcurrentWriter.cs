using System.Collections.Concurrent;

namespace Poltergeist.Modules.Logging;

public class ConcurrentWriter : IDisposable
{
    private readonly Stream FileStream;
    private readonly TextWriter FileWriter;
    private readonly BlockingCollection<string> WritingQueue;
    private bool IsDisposed;

    public ConcurrentWriter(string path)
    {
        var fileInfo = new FileInfo(path);

        try
        {
            if (fileInfo.Directory is not null && fileInfo.Directory.FullName != fileInfo.Directory.Root.FullName)
            {
                fileInfo.Directory.Create();
            }
            FileStream = new FileStream(fileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Write);
            FileWriter = new StreamWriter(FileStream);

            WritingQueue = new(256);

            Task.Factory.StartNew(WriteToFile, this, TaskCreationOptions.LongRunning);
        }
        catch (Exception)
        {
            FileStream?.Dispose();
            FileWriter?.Dispose();
            WritingQueue?.Dispose();

            throw;
        }
    }

    public void WriteLine(string text)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        WritingQueue.Add(text);
    }

    private void WriteToFile(object? state)
    {
        foreach (var message in WritingQueue.GetConsumingEnumerable())
        {
            FileWriter.WriteLine(message);
            FileWriter.Flush();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            FileWriter?.Dispose();
            FileStream?.Dispose();
            WritingQueue?.Dispose();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
