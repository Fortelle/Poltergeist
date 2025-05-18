using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Test;

public partial class ExampleGroup : MacroGroup
{

    [AutoLoad]
    public LoopMacro OutputerExample = new("example_outputservice")
    {
        Title = "Outputer Example",

        Description = "This example shows how to display messages by using ArgumentService.Outputer.",

        IsSingleton = true,

        LoopOptions =
        {
            DefaultCount = 30,
            Instrument = LoopInstrumentType.ProgressBar,
        },

        Iterate = (args) =>
        {
            Thread.Sleep(500);
            if (NumericUtil.IsPrime(args.Index))
            {
                args.Outputer.Write(OutputLevel.Success, $"Found a prime number: {args.Index}");
            }
        }
    };

}
