using Poltergeist.Modules.Events;

namespace Poltergeist.UI.Pages;

public class PageClosedHandler(string pageKey) : AppEventHandler
{
    public string PageKey => pageKey;
}
