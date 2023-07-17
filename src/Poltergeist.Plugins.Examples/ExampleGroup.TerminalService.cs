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

        Configure = (services, _) =>
        {
            services.AddSingleton<TerminalService>();
        },

        Execution = (e) =>
        {
            var cmd = e.Processor.GetService<TerminalService>();
            cmd.Start();
            cmd.Execute("cd");
            cmd.Execute("cd /d c:/");
            cmd.Execute("cd");
            cmd.Execute("dir");
            cmd.Close();
        }

    };

}
