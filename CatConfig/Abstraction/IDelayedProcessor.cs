using CatConfig.CclUnit;

namespace CatConfig;

public interface IDelayedProcessor
{
    string Name { get; }
    string ProtocolSchema { get; }
    IUnit ResolveDelayedUnit(int id, string name, UnitPath path);
}
