using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class MacroLoggerExample : BasicMacro
{
    public MacroLoggerExample() : base()
    {
        Title = nameof(MacroLogger);

        Category = "Features";

        Description = "This example writes log messages at different levels.";

        Execute = (args) =>
        {
            var levels = Enum.GetValues<LogLevel>();
            foreach (var level in levels)
            {
                args.Logger.Log(level, $"This is a log message at <{level}> level.");
            }
        };
    }
}