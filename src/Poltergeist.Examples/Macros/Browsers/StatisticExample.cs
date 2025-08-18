using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class StatisticExample : BasicMacro
{
    public StatisticExample() : base()
    {
        Title = "Statistics";

        Category = "Browsers";

        Description = "This example shows how to define and update the statistic items.";

        StatisticDefinitions.Add(new StatisticDefinition<int>("count1")
        {
            TargetKey = "count",
            Update = (total, next) => total + next,
        });

        StatisticDefinitions.Add(new StatisticDefinition<int>("count2")
        {
            TryUpdate = (int accumulatedValue, ProcessorReport report, out int currentValue) =>
            {
                currentValue = accumulatedValue + report.Get<int>("count");
                return true;
            },
        });

        Execute = args =>
        {
            args.Processor.Report.Add("count", 1);
        };
    }
};
