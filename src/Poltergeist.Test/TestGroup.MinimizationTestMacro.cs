using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public BasicMacro MinimizationTestMacro = new("test_minimization")
    {
        Title = "Minimization Test",

        Description = "This macro is used for testing the app minimization.",

        IsSingleton = true,

        Modules =
        {
            new MinimizationModule(true),
        },

        Execute = (args) =>
        {
            Thread.Sleep(3000);
        }

    };
}
