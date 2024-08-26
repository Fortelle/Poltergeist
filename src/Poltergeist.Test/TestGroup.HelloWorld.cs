using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;
using Poltergeist.Operations.Foreground;

namespace Poltergeist.Test;

public partial class TestGroup : MacroGroup
{

    [AutoLoad]
    public ForegroundMacro ForegroundTest = new("test_foreground")
    {
        Title = "Hello World",

        Description = "Opens a notepad and inputs text.",

        IsSingleton = true,

        Filename = @"notepad",
        Delay = 1000,

        RegionConfig = new()
        {
            ClassName = "Notepad",
            BringToFront = true,
        },

        Iterate = (args, ope) =>
        {
            ope.Keyboard.Input("Hello world!");
        }
    };

}
