using System;
using System.Collections;

namespace Poltergeist.Automations.Configs;

public class OptionItem<T> : IOptionItem
{
    public string Key { get; }
    public T Value { get; set; }
    private T DefaultValue { get; }

    public string DisplayLabel { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; }

    public bool HasChanged { get; set; }



    public OptionItem(string key, T defaultValue)
    {
        Key = key;
        DefaultValue = defaultValue;

        Value = defaultValue;
    }

    public OptionItem(string key) : this(key, default)
    {
        Key = key;
    }

    public bool IsDefault =>
        Value is null ? DefaultValue is not null
        : Value is IEquatable<T> ie ? ie.Equals(DefaultValue)
        : Value.ToString() == DefaultValue.ToString();

    public Type Type => Value.GetType();

    object IOptionItem.Value
    {
        get => Value;
        set => Value = (T)value;
    }
}

public interface IOptionItem
{
    public string Key { get; }

    public string DisplayLabel { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; }

    public bool HasChanged { get; set; }

    public Type Type { get; }
    public string ToString();
    public bool IsDefault { get; }

    public object Value { get; set; }

}


public class FileOptionItem : OptionItem<string>
{
    public FileOptionItem(string key, string defaultValue) : base(key, defaultValue)
    {
    }
}

public class FolderOptionItem : OptionItem<string>
{
    public FolderOptionItem(string key, string defaultValue) : base(key, defaultValue)
    {
    }
}


public class ChoiceOptionItem<T> : OptionItem<T>, IChoiceOptionItem 
{
    public T[] Choices { get; set; }
    IEnumerable IChoiceOptionItem.Choices => Choices;

    public ChoiceOptionItem(string key, T[] choices, T defaultValue) : base(key, defaultValue)
    {
        Choices = choices;
    }
}

public interface IChoiceOptionItem
{
    public IEnumerable Choices { get; }
}
