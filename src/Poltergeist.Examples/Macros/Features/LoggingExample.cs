using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class MacroLoggerExample : CommonOneshotMacroBase
{
    public MacroLoggerExample() : base()
    {
        Title = nameof(MacroLogger);

        Category = "Features";

        Description = "This example writes log messages at different levels.";
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var levels = Enum.GetValues<LogLevel>();
        var logger = controller.Processor.GetService<MacroLogger>();
        foreach (var level in levels)
        {
            logger.Log(level, Title, $"This is a log message at <{level}> level.");
        }
    }
}