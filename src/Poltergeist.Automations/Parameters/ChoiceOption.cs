using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Parameters;

public class ChoiceOption<T> : OptionItem<T>, IChoiceOptionItem
{
    public required ChoiceEntry[] Choices { get; init; }

    public ChoiceOptionMode Mode { get; set; }

    public ChoiceOption(string key, T defaultValue) : base(key, defaultValue)
    {
    }

    [SetsRequiredMembers]
    public ChoiceOption(string key, T[] choices) : base(key, choices[0])
    {
        Choices = choices.Select(x => new ChoiceEntry(x)).ToArray();
    }

    [SetsRequiredMembers]
    public ChoiceOption(string key, T[] choices, T defaultValue) : base(key, defaultValue)
    {
        Choices = choices.Select(x => new ChoiceEntry(x)).ToArray();
    }

    [SetsRequiredMembers]
    public ChoiceOption(string key, Dictionary<T, string> choices, T defaultValue) : base(key, defaultValue)
    {
        Choices = choices.Select(x => new ChoiceEntry(x.Key, x.Value)).ToArray();
    }

    public ChoiceEntry[] GetChoices() => Choices;
}
