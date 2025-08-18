using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class MetadataExample : BasicMacro
{
    public MetadataExample() : base()
    {
        Title = "Metadata";

        Category = "Browsers";

        Description = "This example shows how to define custom metadata variables.";

        Metadata.Add("Example Label", "Example Value");
    }
};
