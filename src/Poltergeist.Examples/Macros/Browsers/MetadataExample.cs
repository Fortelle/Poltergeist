using Poltergeist.Automations.Macros.Oneshots;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class MetadataExample : UnrunnableMacro
{
    public MetadataExample() : base()
    {
        Title = "Metadata";

        Category = "Browsers";

        Description = "This example shows how to define custom metadata variables.";

        Metadata.Add("Example Label", "Example Value");
    }
};
