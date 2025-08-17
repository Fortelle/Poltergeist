using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.App;

/// <summary>
/// Respresent an event that occurs when the application is initialized.
/// </summary>
/// <remarks>
/// This event will be raised after the <see cref="PoltergeistApplication.Host"/> is built and before the <see cref="PoltergeistApplication.MainWindow"/> is created.
/// </remarks>
[StrictOneTime]
public class AppInitializedEvent : AppEvent
{
}
