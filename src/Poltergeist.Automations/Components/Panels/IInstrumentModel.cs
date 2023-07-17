namespace Poltergeist.Automations.Components.Panels;

public interface IInstrumentModel
{
    public string? Key { get; }

    public string? Title { get; }

    public string MacroKey { get; }

    public string ProcessId { get; }

    public bool AutoScroll { get; }

    public bool Stretch { get; }
}
