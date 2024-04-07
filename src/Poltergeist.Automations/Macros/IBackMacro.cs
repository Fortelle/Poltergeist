using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public interface IBackMacro : IFrontBackMacro
{
    public MacroStorage Storage { get; }
    public List<MacroModule> Modules { get; }

    public void Configure(IConfigurableProcessor processor);
    public void Prepare(IPreparableProcessor processor);
}
