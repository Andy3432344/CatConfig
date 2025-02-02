using System.Diagnostics.CodeAnalysis;
using CatConfig.CclUnit;

namespace CatConfig;

public interface IDelayedUnit : IComplexUnit
{
    string Name { get; }
    string GetHostName();
    string GetProtocolSchema();
    UnitPath GetPath(char qtExpand);
    int GetArity();
    string ResolveUrl(params string[] parameters);
}
