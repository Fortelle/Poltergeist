using System.Collections;

namespace Poltergeist.Automations.Configs;

public class IndexOption<T> : OptionItem<int>, IChoiceOptionItem, IIndexOptionItem
{
    public T[] Choices { get; set; }

    public ChoiceOptionMode Mode { get; set; }

    IEnumerable IChoiceOptionItem.Choices => Choices;

    public IndexOption(string key, T[] choices) : base(key, 0)
    {
        Choices = choices;
    }

    public IndexOption(string key, T[] choices, int defaultValue) : base(key, defaultValue)
    {
        Choices = choices;
    }
}
