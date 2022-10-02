using Poltergeist.Automations.Processors;

namespace Poltergeist.Components.Loops;

public class LoopCheckNextArguments : ArgumentService
{
    //public int Index { get; set; }
    //public DateTime BeginTime { get; internal set; }
    //public DateTime EndTime { get; internal set; }
    public CheckNextResult Result { get; set; }

    public LoopCheckNextArguments(MacroProcessor processor) : base(processor)
    {
    }
}
