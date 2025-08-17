using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Pipes;

public class PipeMessageReceivedEvent(PipeMessage message) : AppEvent
{
    public PipeMessage Message => message;
}
