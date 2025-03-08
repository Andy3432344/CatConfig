namespace CatConfig;

public delegate IUnit Function(params object[] args);

public interface IUnitRecord : IUnit, IComplexUnit
{
	string Name { get; }
	string[] FieldNames { get; }

	Function this[IDelayedUnit field] { get; }
	IUnit this[string fieldName] { get; }
	IUnitRecord Transform(IUnitRecord import, bool @override = false);
}
