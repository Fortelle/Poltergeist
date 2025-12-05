using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.App;

public class AppNotificationReceivedEvent : AppEvent
{
    public required IDictionary<string, string> Arguments { get; init; }
}
