﻿using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Settings;

public class AppSettingsChangedHandler() : AppEventHandler
{
    public required string Key { get; init; }
    public object? NewValue { get; init; }
}
