namespace Poltergeist.Automations.Macros;

public class SyncAction : MacroAction
{
    public required Action<SyncActionContext> Execute { get; set; }
}
