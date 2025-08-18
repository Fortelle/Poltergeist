using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class NumberOptionExample : UnrunnableMacro
{
    public NumberOptionExample() : base()
    {
        Title = "Number options";

        Category = "Options";

        Description = "This example defines several options of numeric types.";

        OptionDefinitions.Add(new OptionDefinition<int>("int")
        {
        });

        OptionDefinitions.Add(new NumberOption<int>("int_numberbox")
        {
            Minimum = 0,
            Maximum = 100,
            Layout = NumberOptionLayout.NumberBox,
        });

        OptionDefinitions.Add(new NumberOption<int>("int_slider")
        {
            Minimum = 0,
            Maximum = 100,
            Layout = NumberOptionLayout.Slider,
        });

        OptionDefinitions.Add(new NumberOption<double>("double_numberbox")
        {
            Minimum = 0,
            Maximum = 1,
            StepFrequency = 0.01,
            Layout = NumberOptionLayout.NumberBox,
        });

        OptionDefinitions.Add(new NumberOption<double>("double_slider")
        {
            Minimum = 0,
            Maximum = 1,
            StepFrequency = 0.01,
            Layout = NumberOptionLayout.Slider,
            ValueFormat = "P0",
        });
    }
}
