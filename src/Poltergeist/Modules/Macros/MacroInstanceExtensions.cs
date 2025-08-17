namespace Poltergeist.Modules.Macros;

public static partial class MacroInstanceExtensions
{
    public static string GetPageKey(this MacroInstance instance)
    {
        return MacroManager.GetPageKey(instance.InstanceId);
    }

    public static IReadOnlyDictionary<string, object?> GetEnvironments(this MacroInstance instance)
    {
        var environments = new Dictionary<string, object?>
        {
            {"macro_instance_id", instance.InstanceId},
            {"macro_page_key", instance.GetPageKey()},
        };

        if (!string.IsNullOrEmpty(instance.PrivateFolder))
        {
            environments["private_folder"] = instance.PrivateFolder;
        }

        return environments;
    }

    public static IReadOnlyDictionary<string, object?> GetOptions(this MacroInstance instance)
    {
        var options = new Dictionary<string, object?>();
        foreach (var (definition, value) in PoltergeistApplication.GetService<GlobalOptionsService>().GlobalOptions.GetDefinitionValueCollection())
        {
            options[definition.Key] = value;
        }

        if (instance.Options?.DefinitionCount > 0)
        {
            foreach (var (definition, value) in instance.Options.GetDefinitionValueCollection())
            {
                options[definition.Key] = value;
            }
        }

        return options;
    }

    public static IReadOnlyDictionary<string, string> GetStatistics(this MacroInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance.Template);
        ArgumentNullException.ThrowIfNull(instance.Statistics);

        var statistics = new Dictionary<string, string>();
        var statisticsService = PoltergeistApplication.GetService<MacroStatisticsService>();
        foreach (var definition in instance.Template.StatisticDefinitions)
        {
            var label = definition.DisplayLabel ?? definition.Key;
            var value = definition.IsGlobal
                ? statisticsService.GlobalStatistics.Get(definition.Key)
                : instance.Statistics.Get(definition.Key);
            var text = definition.FormatValue(value);
            statistics.Add(label, text);
        }

        return statistics;
    }

}
