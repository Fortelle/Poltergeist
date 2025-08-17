using Poltergeist.Modules.Events;
using Poltergeist.UI.Windows;

namespace Poltergeist.Modules.App;

/// <summary>
/// Respresent an event that occurs when the <see cref="MainWindow"/> is shown.
/// </summary>
[StrictOneTime]
public class AppWindowActivatedEvent : AppEvent
{
}
