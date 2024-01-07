using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class ExampleGroup : MacroGroup
{

    [AutoLoad]
    public BasicMacro TerminalExample = new("example_terminal")
    {
        Title = "Terminal Example",
        Description = "This example shows how to create a terminal panel and execute commands.",

        Configure = (processor) =>
        {
            processor.Services.AddSingleton<TerminalService>();
        },

        Execute = (args) =>
        {
            var cmd = args.Processor.GetService<TerminalService>();
            cmd.Start();
            cmd.Execute("cd");
            cmd.Execute("cd /d c:/");
            cmd.Execute("cd");
            cmd.Execute("dir");
            cmd.Close();
        }

    };

}
