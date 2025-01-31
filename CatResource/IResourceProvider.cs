using CatConfig;

namespace CatResource;

public interface IResourceProvider
{
    string DataFormat { get; }
    string ResourceName { get; }
    IUnit GetResource(int id, string path);

}