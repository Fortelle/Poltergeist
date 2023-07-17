using System;

namespace Poltergeist.Automations.Components.Panels;

public class ListInstrumentButton
{
    public string? Key { get; set; }
    public string? Text { get; set; }
    public Action? Callback { get; set; }
    public string? Argument { get; set; }
    public int CountdownSeconds { get; set; }
}
