using System;

namespace Poltergeist.Automations.Configs;

public class EnumOption<T> : OptionItem<T?>, IEnumOptionItem where T : Enum
{
    public EnumOption(string key) : base(key, default)
    {
    }

    public EnumOption(string key, T? defaultValue = default) : base(key, defaultValue)
    {
    }
}
