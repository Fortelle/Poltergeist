namespace Poltergeist.Automations.Parameters;

public class IndexChoiceOption<T> : OptionDefinition<int>, IChoiceOption, IIndexChoiceOption
{
    public ChoiceEntry[] Choices { get; set; }

    public ChoiceOptionMode Mode { get; set; }

    public IndexChoiceOption(string key, T[] choices, int defaultValue = 0) : base(key, defaultValue)
    {
        Choices = choices.Select((x, i) => new ChoiceEntry(i, x?.ToString() ?? "")).ToArray();
    }

    public ChoiceEntry[] GetChoices() => Choices;
}
