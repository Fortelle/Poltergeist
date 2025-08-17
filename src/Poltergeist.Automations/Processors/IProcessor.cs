using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IProcessor
{
    public string ProcessorId { get; }

    public ParameterValueCollection Options { get; }
    public ParameterValueCollection Environments { get; }
    public ParameterValueCollection SessionStorage { get; }
    public ParameterValueCollection OutputStorage { get; }
    public ParameterValueCollection Report { get; }
}
