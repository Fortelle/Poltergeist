using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.App;

public class AppWindowClosingHandler() : AppEventHandler
{
    private bool _cancel;
    public bool Cancel
    {
        get => _cancel;
        set => _cancel |= value;
    }

    public string? CancelMessage { get; set; }
}
