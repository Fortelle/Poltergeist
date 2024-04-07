using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Common.Utilities.Maths;

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

        Execute = (args) =>
        {
            Thread.Sleep(500);
            if (NumericUtil.IsPrime(args.Index))
            {
                args.Outputer.Write(OutputLevel.Success, $"Found a prime number: {args.Index}");
            }
        }
    };

}
