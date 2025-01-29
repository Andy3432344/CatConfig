using System.Diagnostics.CodeAnalysis;

namespace CatConfig;

public interface IDelayedUnit : IComplexUnit
{
	string Name { get; }
	string GetHostName();
	string GetProtocolSchema();
	string GetPath();
	int GetArity();
	string ResolveUrl(params string[] parameters);
}
