﻿namespace Poltergeist.Automations.Structures.Parameters;

public class BoolOption : OptionDefinition<bool>
{
    public BoolOptionMode Mode { get; set; }
    public string? Text { get; set; }
    public string? OnText { get; set; }
    public string? OffText { get; set; }

    public BoolOption(string key, bool defaultValue = false) : base(key, defaultValue)
    {
    }
}
