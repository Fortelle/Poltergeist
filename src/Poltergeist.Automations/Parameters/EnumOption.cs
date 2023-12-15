using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Parameters;

public class EnumOption<T> : OptionItem<T>, IChoiceOptionItem, IEnumOptionItem where T : Enum
{
    public Func<T, string>? GetText { get; set; }

    public ChoiceOptionMode Mode { get; set; }

    public EnumOption(string key) : base(key, default)
    {
    }

    public EnumOption(string key, T defaultValue = default) : base(key, defaultValue)
    {
    }

    public ChoiceEntry[] GetChoices()
    {
        return Enum.GetValues(typeof(T)).OfType<T>().Select(x => new ChoiceEntry(x, GetText?.Invoke(x) ?? x.ToString())).ToArray();
    }
}
