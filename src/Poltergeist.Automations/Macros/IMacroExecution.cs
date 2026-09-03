using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public interface IMacroExecution : IMacroBase
{
    List<MacroModule> Modules { get; }

    void OnProcessorCreated(IMacroProcessorInformation processor);
    bool CanExecute([MaybeNullWhen(true)] out string invalidationMessage);
    bool CanExecute(IMacroProcessorInformation processor, [MaybeNullWhen(true)] out string invalidationMessage);
    void RegisterServices(ServiceCollection services, RegisterServicesArguments arguments);
}