﻿using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Pages.Macros;

public class MacroStartArguments
{
    public required string MacroKey { get; init; }
    public required LaunchReason Reason { get; init; }
    public ConfigVariation? Variation { get; init; }
}