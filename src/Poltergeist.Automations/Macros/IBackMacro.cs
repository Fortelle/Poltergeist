using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IBackMacro : IFrontBackMacro
{
    public ParameterValueCollection ExtraData { get; }
    public List<MacroModule> Modules { get; }

    public void Configure(IConfigurableProcessor processor);
    public void Prepare(IPreparableProcessor processor);
}
