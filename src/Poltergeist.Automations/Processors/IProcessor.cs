using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IProcessor
{
    string ProcessorId { get; }

    ParameterValueCollection Options { get; }
    ParameterValueCollection Environments { get; }
    ParameterValueCollection SessionStorage { get; }
    ParameterValueCollection OutputStorage { get; }
    ParameterValueCollection Report { get; }
}
