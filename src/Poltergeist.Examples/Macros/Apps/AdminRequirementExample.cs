using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class AdminRequirementExample : BasicMacro
{
    public AdminRequirementExample() : base()
    {
        Title = "Admin Requirement";

        Category = "Apps";

        Description = "A macro that requires administrator privileges to run.";

        RequiresAdmin = true;

        Execute = (args) =>
        {
            args.Outputer.Write($"Hello world!");
        };
    }
}
