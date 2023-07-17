using System.IO.Pipes;

namespace Poltergeist.Helpers;

internal static class SingletonHelper
{
    private const string AppKey = "Poltergeist";
    private const string MutexKey = $"{AppKey}_Singleton_Mutex";
    private const string PipeKey = $"{AppKey}_Pipe";

    private static readonly Mutex mutex = new(true, MutexKey);

    private static NamedPipeServerStream? pipeServer;

    public static bool IsSingleInstance => mutex.WaitOne(TimeSpan.Zero);

    public static event Action<string>? ArgumentReceived;

    public static void Load()
    {
        pipeServer = new NamedPipeServerStream(PipeKey, PipeDirection.InOut , 1);
        ThreadPool.QueueUserWorkItem(HandleClient);
    }

    private static void HandleClient(object? state)
    {
        pipeServer!.WaitForConnection();
        var reader = new StreamReader(pipeServer);
        var argument = reader.ReadToEnd();
        pipeServer.Disconnect();

        ArgumentReceived?.Invoke(argument);

        ThreadPool.QueueUserWorkItem(HandleClient);
    }

    public static void SendArggument(string argument)
    {
        using var client = new NamedPipeClientStream(".", PipeKey);
        client.Connect();

        using var writer = new StreamWriter(client);
        writer.Write(argument);
        writer.Flush();
    }

}
