using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.App;

/// <summary>
/// Respresent an event that occurs when the <see cref="MainWindow"/> is closed.
/// </summary>
[StrictOneTime]
public class AppWindowClosedEvent : AppEvent
{
}
