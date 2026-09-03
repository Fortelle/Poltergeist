using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
[ServiceDependency<TerminalService>(ServiceLifetime.Singleton)]
public class TerminalExample : CommonOneshotMacroBase
{
    public TerminalExample() : base()
    {
        Title = nameof(TerminalService);

        Category = "Features";

        Description = $"This example uses the {nameof(TerminalService)} to execute commands.";
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var cmd = controller.Processor.GetService<TerminalService>();
        cmd.Start();
        cmd.Execute("cd");
        cmd.Execute("cd /d c:/");
        cmd.Execute("cd");
        cmd.Execute("dir");
        cmd.Close();
    }
}
