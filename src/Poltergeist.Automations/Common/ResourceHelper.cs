namespace Poltergeist.Automations.Common;

public static class ResourceHelper
{
    public static string Localize(string key, params object?[] args)
    {
        var parts = key.Split('/');
        var mapKey = string.Join('/', parts[..^1]);
        var resourceKey = parts[^1];

        var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse(mapKey);
        var resource = resourceLoader.GetString(resourceKey);

        if (resource is not null && args.Length > 0)
        {
            resource = string.Format(resource, args);
        }

        resource ??= '{' + resourceKey + '}';

        return resource;
    }
}
