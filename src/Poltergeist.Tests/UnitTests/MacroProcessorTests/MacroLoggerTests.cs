using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroProcessorTests;

[TestClass]
public class MacroLoggerTests
{
    [TestMethod]
    public void Test()
    {
        var macro = new BasicMacro
        {
            Execute = (args) =>
            {
                var levels = Enum.GetValues<LogLevel>();
                foreach (var level in levels)
                {
                    args.Processor.GetService<MacroLogger>().Log(level, "", "test_log");
                }
            },
        };
        var args = new MacroProcessorArguments()
        {
            Options = new()
            {
                {MacroLogger.ToProcessorLevelKey, LogLevel.All}
            }
        };

        var processor = new MacroProcessor(macro, args);

        var checkList = Enum.GetValues<LogLevel>().ToDictionary(x => x, _ => false);

        processor.LogWritten += (s, e) =>
        {
            if (e.Entry.Message == "test_log")
            {
                checkList[e.Entry.Level] = true;
            }
        };

        processor.Execute();

        Assert.IsTrue(checkList.All(x => x.Value));
    }
}
