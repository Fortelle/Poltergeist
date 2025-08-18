using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Examples;

[ExampleMacro]
public class LoopMacroExample : LoopMacro
{
    public LoopMacroExample() : base()
    {
        Title = nameof(LoopMacro);

        Category = "Macros";

        Description = $"A macro that can be repeatedly executed.";

        OptionDefinitions.Add(new EnumOption<LoopInstrumentType>("loop-instrument", LoopInstrumentType.List)
        {
            DisplayLabel = "Instrument Style",
            Category = LocalizationUtil.Localize("Loops_Category"),
        });

        LoopOptions.DefaultCount = 10;

        LoopOptions.Instrument = LoopInstrumentType.List;

        Iterate = (proc) =>
        {
            Thread.Sleep(100);
        };
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        LoopOptions.Instrument = processor.Options.Get<LoopInstrumentType>("loop-instrument");
    }

}
