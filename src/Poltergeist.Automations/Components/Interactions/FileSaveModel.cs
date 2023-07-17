using System.Collections.Generic;

namespace Poltergeist.Automations.Components.Interactions;

public class FileSaveModel : InteractionModel
{
    public Dictionary<string, string[]>? Filters { get; set; }

    public string? SuggestedFileName { get; set; }

    public string? FileName { get; set; }
}
