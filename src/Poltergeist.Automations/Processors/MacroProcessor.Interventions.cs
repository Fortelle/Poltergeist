namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    public bool Intervene(string interventionKey)
    {
        var intervention = Macro.Interventions.FirstOrDefault(x => x.Key == interventionKey);
        
        if (intervention is null)
        {
            Logger?.Warn($"Failed to inject intervention '{interventionKey}': key not found.");

            return false;
        }

        if (intervention.IsDevelopmentOnly && !Environments.GetValueOrDefault<bool>("is_development"))
        {
            Logger?.Warn($"Failed to inject intervention '{interventionKey}': development only.");

            return false;
        }

        if (intervention.CanIntervene?.Invoke(this) == false)
        {
            Logger?.Warn($"Failed to inject intervention '{interventionKey}': cannot handle.");

            return false;
        }

        foreach (var (key, value) in intervention.Variables)
        {
            SessionStorage.AddOrUpdate(key, value);
            Logger?.Trace($"Added intervention variable '{key}' = '{value}'.");
        }

        Logger?.Info($"Injected intervention '{interventionKey}'.");

        return true;
    }
}
