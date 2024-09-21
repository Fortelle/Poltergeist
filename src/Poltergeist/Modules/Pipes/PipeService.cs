using System.IO.Pipes;
using System.Text.Json;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Pipes;

public class PipeService : ServiceBase
{
    private const string PipeKey = $"Poltergeist_Pipe";

    private PipeServer? Server;

    public PipeService(AppEventService eventService)
    {
        eventService.Subscribe<AppWindowLoadedHandler>(OnAppWindowLoaded);
        eventService.Subscribe<AppWindowClosedHandler>(OnAppWindowClosed);
    }

    private void OnAppWindowLoaded(AppWindowLoadedHandler handler)
    {
        Server = new(PipeKey);
        Server.MessageReceived += Server_MessageReceived;
    }

    private void OnAppWindowClosed(AppWindowClosedHandler handler)
    {
        if (Server is not null)
        {
            Server.MessageReceived -= Server_MessageReceived;
            Server.Close();
        }
    }

    private void Server_MessageReceived(string message)
    {
        Logger.Trace("Pipe message received.", new { message });

        PipeMessage? pipemsg;
        try
        {
            pipemsg = JsonSerializer.Deserialize<PipeMessage>(message)!;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to deserialize pipe message: {ex.Message}");
            return;
        }
        PoltergeistApplication.GetService<AppEventService>().Raise<PipeMessageReceivedHandler>(new(pipemsg));
    }

    public static void Send(string key, object? value)
    {
        var pipemsg = new PipeMessage()
        {
            Key = key,
            Value = value is null ? null : JsonSerializer.SerializeToNode(value),
        };
        var json = JsonSerializer.Serialize(pipemsg);

        using var client = new NamedPipeClientStream(".", PipeKey);
        client.Connect();

        using var writer = new StreamWriter(client);
        writer.Write(json);
        writer.Flush();
    }

}
