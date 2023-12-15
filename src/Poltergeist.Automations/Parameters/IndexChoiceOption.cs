namespace Poltergeist.Automations.Parameters;

public class IndexChoiceOption<T> : OptionItem<int>, IChoiceOptionItem, IIndexChoiceOptionItem
{
    public ChoiceEntry[] Choices { get; set; }

    public ChoiceOptionMode Mode { get; set; }

    public IndexChoiceOption(string key, T[] choices, int defaultValue = 0) : base(key, defaultValue)
    {
        Choices = choices.Select((x, i) => new ChoiceEntry(i, x.ToString() ?? "")).ToArray();
    }

    public ChoiceEntry[] GetChoices() => Choices;
}
