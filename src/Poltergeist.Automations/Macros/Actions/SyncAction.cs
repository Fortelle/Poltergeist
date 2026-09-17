using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Macros;

public class SyncAction : MacroAction
{
    public required Action<SyncActionContext> Execute { get; set; }

    public SyncAction()
    {
        Icon = IconInfo.FromGlyph("\uE768");
    }
}
