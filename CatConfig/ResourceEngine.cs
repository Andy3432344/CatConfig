using CatConfig.CclUnit;

namespace CatConfig;

public static class ResourceEngine
{
	private static Dictionary<string, Dictionary<string, IResourceProvider>>
		dataModules = new(StringComparer.OrdinalIgnoreCase);

	private static Dictionary<string, Dictionary<string, IResourceProvider>>
		extensionModules = new(StringComparer.OrdinalIgnoreCase);

	public static bool RegisterProvider(IResourceProvider provider)
	{
		var dic = provider.DataFormat.Equals("mod", StringComparison.OrdinalIgnoreCase) 
			? dataModules 
			: extensionModules;

		return RegisterProvider(provider, dic);

	}

	private static bool RegisterProvider(IResourceProvider provider, Dictionary<string, Dictionary<string, IResourceProvider>> dictionary)
	{
		if (!dictionary.TryGetValue(provider.ResourceType, out var providers))
			dictionary[provider.ResourceType] = providers = new(StringComparer.OrdinalIgnoreCase);
		return providers.TryAdd(provider.ResourceName, provider);
	}

	public static IUnit GetResource(IUnit request, string format, string resourceType, string resourceName, UnitPath path)
	{
		var dic = format.Equals("mod", StringComparison.OrdinalIgnoreCase) ? dataModules : extensionModules;

		return GetResource(request, resourceType, resourceName, path, dic);


	}

	private static IUnit GetResource(IUnit request, string resourceType, string resourceName, UnitPath path, Dictionary<string, Dictionary<string, IResourceProvider>> dictionary)
	{
		if (dictionary.TryGetValue(resourceType, out var resources))
			if (resources.TryGetValue(resourceName, out var provider))
				return provider.GetResource(request, path);

		return new NoValue(request.Level);
	}
}