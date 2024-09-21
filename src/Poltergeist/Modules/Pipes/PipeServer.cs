using System.IO.Pipes;

namespace Poltergeist.Modules.Pipes;

public class PipeServer : IDisposable
{
    public event Action<string>? MessageReceived;

    private readonly NamedPipeServerStream PipeStream;
    private readonly CancellationTokenSource CancellationTokenSource;
    private bool IsDisposed;

    public PipeServer(string pipeKey)
    {
        PipeStream = new NamedPipeServerStream(pipeKey, PipeDirection.InOut, 1);
        CancellationTokenSource = new CancellationTokenSource();
        ThreadPool.QueueUserWorkItem(HandleClient);
    }

    private async void HandleClient(object? state)
    {
        try
        {
            await PipeStream.WaitForConnectionAsync(CancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        var reader = new StreamReader(PipeStream);
        var message = reader.ReadToEnd();
        PipeStream.Disconnect();
        MessageReceived?.Invoke(message);

        ThreadPool.QueueUserWorkItem(HandleClient);
    }

    public void Close()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
            PipeStream?.Close();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
