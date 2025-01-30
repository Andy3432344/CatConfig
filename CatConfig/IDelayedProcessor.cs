namespace CatConfig;

public interface IDelayedProcessor
{
	string Name { get; }
	string ProtocolSchema { get; }

	//IUnit ResolveDelayedUnit(IDelayedUnit delayed);
    IUnit ResolveDelayedUnit(int id, string name, string path);
}
