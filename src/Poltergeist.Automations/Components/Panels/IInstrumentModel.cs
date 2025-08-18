namespace Poltergeist.Automations.Components.Panels;

public interface IInstrumentModel
{
    string? Key { get; }

    string? Title { get; }

    string MacroKey { get; }

    string ProcessorId { get; }

    bool AutoScroll { get; }

    bool Stretch { get; }

    bool IsSticky { get; }
}
