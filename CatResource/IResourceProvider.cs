using CatConfig;

namespace CatResource;

public interface IResourceProvider
{
    string DataType { get; }
    string DataFormat { get; }
    IUnit GetResource(int id, string path);

}