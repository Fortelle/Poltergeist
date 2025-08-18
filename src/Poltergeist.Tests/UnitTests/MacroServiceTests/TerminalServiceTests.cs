using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroServiceTests;

[TestClass]
public class TerminalServiceTests // is it really needed?
{
    [TestMethod]
    public void Test()
    {
        const string testText = "test_text";
        var result = "";

        var macro = new BasicMacro()
        {
            Configure = (processor) =>
            {
                processor.Services.AddSingleton<TerminalService>();
            },
            Execute = (args) =>
            {
                var cmd = args.Processor.GetService<TerminalService>();
                cmd.Start();
                result = cmd.Execute($"echo {testText}").Trim();
                cmd.Close();
            }
        };

        MacroProcessor.Execute(macro);

        Assert.AreEqual(testText, result);
    }
}
