using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Macros;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.MacroInstanceTests;

[TestClass]
public class StatisticTests
{
    [UITestMethod]
    public async Task TestSave()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro("statistic_test")
        {
            StatisticDefinitions =
            {
                new StatisticDefinition<int>("count")
                {
                    TargetKey = "count",
                    Update = (total, next) => total + next,
                }
            },
            Execute = processor =>
            {
                processor.Report.Add("count", 1);
            },
        };

        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        var filepath = Path.Combine(PoltergeistApplication.Paths.DocumentDataFolder, "Tests", macro.Key, "statistics.json");
        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }

        for (var i = 0; i <= 1; i++)
        {
            var instance = new MacroInstance(macro)
            {
                IsPersistent = true,
                PrivateFolder = Path.Combine(PoltergeistApplication.Paths.DocumentDataFolder, "Tests", macro.Key),
            };
            instance.Load();

            using var processor = macroManager.CreateProcessor(instance);
            macroManager.Launch(processor, instance);
            var result = processor.GetResult();

            Thread.Sleep(500);

            Assert.AreEqual(i + 1, instance.Statistics!.Get<int>("count"));
        }
    }

    [UITestMethod]
    public async Task TestGlobalStatistics()
    {
        await UITestHelper.WaitForAppWindowLoaded();
        var globalKey = "test_global_count";

        var macro = new TestMacro("statistic_test")
        {
            StatisticDefinitions =
            {
                new StatisticDefinition<int>(globalKey, 100)
                {
                    TargetKey = "count",
                    Update = (total, next) => total + next,
                    IsGlobal = true,
                }
            },
            Execute = processor =>
            {
                processor.Report.Add("count", 1);
            },
        };

        var statisticsService = PoltergeistApplication.GetService<MacroStatisticsService>();
        var templateManager = PoltergeistApplication.GetService<MacroTemplateManager>();
        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        // ensures the global entry is not present
        statisticsService.GlobalStatistics.Remove(globalKey);
        Assert.IsFalse(statisticsService.GlobalStatistics.ContainsDefinition(globalKey));

        // registers the global entry
        templateManager.Register(macro);
        Assert.IsTrue(statisticsService.GlobalStatistics.ContainsDefinition(globalKey));
        Assert.AreEqual(100, statisticsService.GlobalStatistics.Get<int>(globalKey));

        // ensures the global entry is not in the instance statistics
        var instance = new MacroInstance(macro, macro.Key);
        instance.Load();
        Assert.IsFalse(instance.Statistics!.ContainsDefinition(globalKey));

        using var processor = macroManager.CreateProcessor(instance);
        macroManager.Launch(processor, instance);
        var result = processor.GetResult();

        Thread.Sleep(500);

        Assert.AreEqual(101, statisticsService.GlobalStatistics.Get<int>(globalKey));

        statisticsService.GlobalStatistics.Remove(globalKey);
    }

}
