using Microsoft.Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources;

namespace Poltergeist.Automations.Utilities;

public static class LocalizationUtil
{
    private static readonly ResourceManager ResourceManager = new();

    public static string Localize(string key, params object?[] args)
    {
        key = "Poltergeist.Automations/Resources/" + key;
        var value = ResourceManager.MainResourceMap.TryGetValue(key);

        if (value is null)
        {
#if DEBUG
            return '{' + key + '}';
#else
            return "";
#endif
        }

        var text = value.ValueAsString;

        if (args.Length > 0)
        {
            text = string.Format(text, args);
        }

        return text;
    }
}
