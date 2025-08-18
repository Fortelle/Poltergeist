using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IBackMacro : IFrontBackMacro
{
    ParameterValueCollection ExtraData { get; }
    List<MacroModule> Modules { get; }

    void Configure(IConfigurableProcessor processor);
    void Prepare(IPreparableProcessor processor);
}
