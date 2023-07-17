using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Configs;

public class ChoiceOption<T> : OptionItem<T>, IChoiceOptionItem
{
    public required T[] Choices { get; init; }

    public ChoiceOptionMode Mode { get; set; }

    IEnumerable IChoiceOptionItem.Choices => Choices;

    public ChoiceOption(string key, T defaultValue) : base(key, defaultValue)
    {
    }

    [SetsRequiredMembers]
    public ChoiceOption(string key, T[] choices) : base(key, choices[0])
    {
        Choices = choices;
    }

    [SetsRequiredMembers]
    public ChoiceOption(string key, T[] choices, T defaultValue) : base(key, defaultValue)
    {
        Choices = choices;
    }
}
