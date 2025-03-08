using CatConfig.CclUnit;

namespace CatConfig;

public interface IResourceProvider
{
    string DataFormat { get; }
    string ResourceType { get; }
	string ResourceName { get;  }
	IUnit GetResource(IUnit request, UnitPath path);

}