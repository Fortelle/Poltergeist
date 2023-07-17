using System;

namespace Poltergeist.Automations.Components.FlowBuilders;

public class FlowBuilderItem
{
    public string? Text { get; set; }
    public string? Subtext { get; set; }

    public string? StartingText { get; set; }
    public string? EndingText { get; set; }
    public string? IdleText { get; set; }
    public string? ErrorText { get; set; }

    public required Action<FlowBuilderArguments> Execution { get; init; }
}
