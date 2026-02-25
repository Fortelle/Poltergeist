using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Processors;

public class ProcessorIntervention
{
    public required string Key { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public IconInfo? Icon { get; set; }

    public bool IsDevelopmentOnly { get; set; }

    public Func<IUserProcessor, bool>? CanIntervene { get; set; }

    public required Dictionary<string, object> Variables { get; set; }

    public string? Message { get; set; }
}
