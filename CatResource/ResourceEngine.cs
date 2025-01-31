using System.Diagnostics;
using System.Resources;
using System.Text;
using CatConfig;

namespace CatResource;

public static class ResourceEngine
{
    private static Dictionary<string, Dictionary<string, IResourceProvider>>
        providers = new(StringComparer.OrdinalIgnoreCase);

    public static bool RegisterProvider(IResourceProvider provider)
    {
        if (!providers.TryGetValue(provider.DataFormat, out var schemaProcs))
            schemaProcs = providers[provider.DataFormat] = new(StringComparer.OrdinalIgnoreCase);
        return schemaProcs.TryAdd(provider.ResourceName, provider);

    }




}