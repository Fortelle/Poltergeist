using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Components.Storages;

public class SessionStorageService : MacroService
{
    public ParameterValueCollection Storage => Processor.SessionStorage;

    public SessionStorageService(MacroProcessor processor) : base(processor)
    {
    }
}
