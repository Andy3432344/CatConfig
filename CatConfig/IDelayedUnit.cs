namespace CatConfig;

public interface IDelayedUnit : IComplexUnit
{
	string Name { get; }
	string GetHostName();
	string GetProtocolSchema();
	string GetPath();
	IDelayedUnit ResolveUrl(Func<int, string, string, IUnitRecord> resolver, string[] fields);
}
