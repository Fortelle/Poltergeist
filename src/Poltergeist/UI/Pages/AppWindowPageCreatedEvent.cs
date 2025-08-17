using Poltergeist.Modules.Events;

namespace Poltergeist.UI.Pages;

public class AppWindowPageCreatedEvent(string pageKey) : AppEvent
{
    public string PageKey => pageKey;
}
