namespace CatConfig;

public delegate IUnit Function(params object[] args);
public interface IUnitRecord : IComplexUnit
{
	string Name { get; }
	string[] FieldNames { get; }

	Function this[IDelayedUnit field] { get; }
	IUnit this[string fieldName] { get; }
}
