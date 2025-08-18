using System.Collections.Generic;

namespace Poltergeist.Automations.Components.Interactions;

public class FileOpenModel : InteractionModel
{
    public List<string>? Filters { get; set; }

    public bool Multiselect { get; set; }

    public string[]? FileNames { get; set; }
}
