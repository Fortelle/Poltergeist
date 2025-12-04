using Windows.ApplicationModel.Resources;

namespace Poltergeist.Helpers;

// https://github.com/microsoft/WindowsAppSDK/issues/5746
// https://github.com/microsoft/WindowsAppSDK/issues/5832
public static class ResourceHelper
{
    private static readonly Dictionary<string, ResourceLoader> ResourceLoaders = new();

    public static string Localize(string key, params object?[] args)
    {
        var parts = key.Split('/');
        var mapKey = string.Join('/', parts[..^1]);
        var resourceKey = parts[^1];

        if (!ResourceLoaders.TryGetValue(mapKey, out var resourceLoader))
        {
            resourceLoader = ResourceLoader.GetForViewIndependentUse(mapKey);
            ResourceLoaders[mapKey] = resourceLoader;
        }
        var resource = resourceLoader.GetString(resourceKey);

        if (resource is not null && args.Length > 0)
        {
            resource = string.Format(resource, args);
        }

        resource ??= '{' + resourceKey + '}';

        return resource;
    }
}
