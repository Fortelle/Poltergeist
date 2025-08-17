using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Settings;

public class AppSettingsChangedEvent : AppEvent
{
    public required string Key { get; init; }
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
