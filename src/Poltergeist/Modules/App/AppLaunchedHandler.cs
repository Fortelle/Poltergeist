using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.App;

/// <summary>
/// Represents an event that occurs when the launching sequence of the application is completed.
/// </summary>
[StrictOneTime]
public class AppLaunchedEvent : AppEvent
{
}
