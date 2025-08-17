using Poltergeist.Modules.Events;
using Poltergeist.UI.Windows;

namespace Poltergeist.Modules.App;

/// <summary>
/// Represents an event that occurs when the <see cref="ShellPage"/> is set to the <see cref="MainWindow"/>.
/// </summary>
[StrictOneTime]
internal class AppShellPageLoadedEvent : AppEvent
{
}
