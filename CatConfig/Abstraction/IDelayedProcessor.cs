using CatConfig.CclUnit;

namespace CatConfig;

public interface IDelayedProcessor
{
    string Name { get; }
    string ProtocolSchema { get; }
    IUnit ResolveDelayedUnit(IUnit request, string name, UnitPath path);
}
