using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Loops;

namespace Poltergeist.Test;

public partial class TestGroup
{

    [AutoLoad]
    public LoopMacro LoopMacroTest = new("test_loopmacro")
    {
        Title = "LoopMacro Test",
        Description = "This macro is used for testing the LoopMacro.",
        IsSingleton = true,

        LoopOptions =
        {
            DefaultCount = 10,
            Instrument = LoopInstrumentType.List,
        },

        Execute = (proc) =>
        {
            Thread.Sleep(500);
        },

    };

}
