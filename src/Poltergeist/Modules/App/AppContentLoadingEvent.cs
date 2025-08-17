using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.App;

/// <summary>
/// Respresent an event that occurs when the <see cref="PoltergeistApplication"/> is loading its content.
/// </summary>
[StrictOneTime]
public class AppContentLoadingEvent : AppEvent
{
}
