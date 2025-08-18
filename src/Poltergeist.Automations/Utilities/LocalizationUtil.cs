using Windows.ApplicationModel.Resources;

namespace Poltergeist.Automations.Utilities;

public static class LocalizationUtil
{
    private static readonly ResourceLoader ResourceLoader;

    static LocalizationUtil()
    {
        ResourceLoader = ResourceLoader.GetForViewIndependentUse("Poltergeist.Automations/Resources");
    }

    public static string Localize(string key, params object?[] args)
    {
        var resource = ResourceLoader.GetString(key);

        if (resource is not null && args.Length > 0)
        {
            resource = string.Format(resource, args);
        }

        resource ??= '{' + key + '}';

        return resource;
    }
}
