using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IProcessor
{
    public string ProcessId { get; }

    public ParameterValueCollection Options { get; }
    public ParameterValueCollection Environments { get; }
    public ParameterValueCollection Statistics { get; }
    public ParameterValueCollection SessionStorage { get; }
}
