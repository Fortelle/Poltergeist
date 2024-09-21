using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Pipes;

public class PipeMessageReceivedHandler(PipeMessage message) : AppEventHandler
{
    public PipeMessage Message => message;
}
