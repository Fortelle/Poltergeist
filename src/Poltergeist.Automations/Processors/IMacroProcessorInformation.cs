using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IMacroProcessorInformation
{
    string ProcessorId { get; }

    IMacroInformation Macro { get; }

    ProcessorStatus Status { get; }

    ParameterValueCollection Options { get; }
    ParameterValueCollection Environments { get; }
    ParameterValueCollection SessionStorage { get; }
    ParameterValueCollection OutputStorage { get; }
    ParameterValueCollection Report { get; }
}
