using Poltergeist.Modules.Events;

namespace Poltergeist.UI.Pages;

public class PageCreatedHandler(string pageKey) : AppEventHandler
{
    public string PageKey => pageKey;
}
