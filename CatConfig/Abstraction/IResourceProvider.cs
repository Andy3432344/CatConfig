using CatConfig.CclUnit;

namespace CatConfig;

public interface IResourceProvider
{
    string DataFormat { get; }
    string ResourceName { get; }
    IUnit GetResource(int id, UnitPath path);

}