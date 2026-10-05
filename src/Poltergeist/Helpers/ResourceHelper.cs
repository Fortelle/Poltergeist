using Microsoft.Windows.ApplicationModel.Resources;

namespace Poltergeist.Helpers;

// https://github.com/microsoft/WindowsAppSDK/issues/5746
// https://github.com/microsoft/WindowsAppSDK/issues/5832
public static class ResourceHelper
{
    private static readonly ResourceManager ResourceManager = new();

    public static string Localize(string key, params object?[] args)
    {
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
