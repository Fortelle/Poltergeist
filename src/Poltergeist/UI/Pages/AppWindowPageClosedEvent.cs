using Poltergeist.Modules.Events;

namespace Poltergeist.UI.Pages;

public class AppWindowPageClosedEvent(string pageKey) : AppEvent
{
    public string PageKey => pageKey;
}
