using System.Collections;

namespace Poltergeist.Automations.Configs;

public class ChoiceOptionItem<T> : OptionItem<T>, IChoiceOptionItem 
{
    public T[] Choices { get; set; }
    IEnumerable IChoiceOptionItem.Choices => Choices;

    public ChoiceOptionItem(string key, T[] choices, T defaultValue) : base(key, defaultValue)
    {
        Choices = choices;
    }
}
