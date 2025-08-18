using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public partial class TerminalExample : BasicMacro
{
    public TerminalExample() : base()
    {
        Title = nameof(TerminalService);

        Category = "Features";

        Description = $"This example uses the {nameof(TerminalService)} to execute commands.";

        Configure = (processor) =>
        {
            processor.Services.AddSingleton<TerminalService>();
        };

        Execute = (args) =>
        {
            var cmd = args.Processor.GetService<TerminalService>();
            cmd.Start();
            cmd.Execute("cd");
            cmd.Execute("cd /d c:/");
            cmd.Execute("cd");
            cmd.Execute("dir");
            cmd.Close();
        };
    }
}
