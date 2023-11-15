namespace Poltergeist.Automations.Common.Structures;

public class MultilingualEntry : Dictionary<string, string>
{
    public static readonly string Language = Thread.CurrentThread.CurrentUICulture.Name;

    public string? DefaultValue { get; set; }

    public static implicit operator MultilingualEntry(string text)
    {
        return new MultilingualEntry()
        {
            DefaultValue = text,
        };
    }

    public static implicit operator string?(MultilingualEntry me)
    {
        var langCode = Language;

        if (me.TryGetValue(langCode, out var value))
        {
            return value;
        }

        if (langCode.Contains('-'))
        {
            langCode = langCode.Split("-")[0];

            if (me.TryGetValue(langCode, out value))
            {
                return value;
            }

            var key = me.Values.FirstOrDefault(x => x.StartsWith(langCode + '-'));
            if (key is not null)
            {
                return me[key];
            }
        }

        if (me.TryGetValue("en", out value))
        {
            return value;
        }

        var key2 = me.Keys.FirstOrDefault(x => x.StartsWith("en-"));
        if (key2 is not null)
        {
            return me[key2];
        }

        return me.DefaultValue;
    }

}
