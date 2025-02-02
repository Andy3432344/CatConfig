using CatConfig;
using CatConfig.CclUnit;

namespace CatResource;

public static class ResourceEngine
{
    private static Dictionary<string, Dictionary<string, IResourceProvider>>
        providers = new(StringComparer.OrdinalIgnoreCase);

    public static bool RegisterProvider(IResourceProvider provider)
    {
        if (!providers.TryGetValue(provider.DataFormat, out var schemaProcs))
            providers[provider.DataFormat] = schemaProcs = new(StringComparer.OrdinalIgnoreCase);
        return schemaProcs.TryAdd(provider.ResourceName, provider);

    }


    public static IUnit GetResource(int id, string format, string resource, UnitPath path)
    {

        if (providers.TryGetValue(format, out var resources))
            if (resources.TryGetValue(resource, out var provider))
                return provider.GetResource(id, path);

        return new NoValue();

    }

}