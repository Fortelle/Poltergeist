using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Components.Storages;

public class RuntimeStorageService : MacroService
{
    private static readonly ParameterValueCollection _runtimeStorage = new();

#pragma warning disable CA1822 // Mark members as static
    public ParameterValueCollection Storage => _runtimeStorage;
#pragma warning restore CA1822 // Mark members as static

    public RuntimeStorageService(MacroProcessor processor) : base(processor)
    {
    }
}
