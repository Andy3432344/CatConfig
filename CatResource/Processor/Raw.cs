using CatConfig;

namespace CatResource.Processor;

public class Raw : IDelayedProcessor
{
    private NoValue noValue = new();
    Dictionary<string, IResourceProvider> providers = new(StringComparer.OrdinalIgnoreCase);

    public string Name => nameof(Raw);
    public string ProtocolSchema => "res";

    public IUnit ResolveDelayedUnit(int id, string name, string path)
    {
        path = path.Trim('/');
        int index = 0;

        string format = PathHelpers.GetPathNode(path, ref index);

        if (path.Length >= index && providers.TryGetValue(format, out var provider))
            return provider.GetResource(id, path[index..]);

        return noValue;
    }

    public bool RegisterProcessor(IResourceProvider proc)
    {
        return proc.ResourceName == this.Name &&
            providers.TryAdd(proc.DataFormat, proc);
    }

}
