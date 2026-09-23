using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

// todo: support Func<IMacroInformation, ChoiceEntry[]>
public class DynamicChoiceOption<T> : OptionDefinition<T>, IDynamicChoiceOption where T : notnull
{
    public required Func<ChoiceEntry[]> GetChoices { get; init; }

    public DynamicChoiceOption(string key) : base(key)
    {
    }

    [SetsRequiredMembers]
    public DynamicChoiceOption(string key, Func<T[]> getChoices) : base(key)
    {
        GetChoices = () => getChoices().Select(x => new ChoiceEntry(x)).ToArray();
    }

    [SetsRequiredMembers]
    public DynamicChoiceOption(string key, Func<ChoiceEntry[]> getChoices) : base(key)
    {
        GetChoices = getChoices;
    }

    ChoiceEntry[] IDynamicChoiceOption.GetChoices()
    {
        return GetChoices.Invoke();
    }
}
