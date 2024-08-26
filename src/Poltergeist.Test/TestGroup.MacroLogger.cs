using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public BasicMacro MacroLoggerTest = new("test_macrologger")
    {
        Title = "Log Test",
        Description = "This macro is used for testing the MacroLogger service.",
        IsSingleton = true,

        Execute = (args) =>
        {
            var logger = args.Processor.GetService<MacroLogger>();
            foreach (var level in Enum.GetValues<LogLevel>())
            {
                logger.Log(level, "", $"Test log line: {{{level}}}.");
            }
        }
    };


}
