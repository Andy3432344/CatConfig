using CatConfig;

namespace CatConfig.CclUnit;

public class UnitRecord : IUnitRecord
{
	private readonly Dictionary<string, IUnit> tree;
	private readonly Func<IDelayedUnit, IUnit> resolve;

	public UnitRecord(int id, string name, int level, Dictionary<string, IUnit> tree, Func<IDelayedUnit, IUnit> resolver)
	{
		Id = id;
		Level = level;
		FieldNames = tree.Keys.ToArray();
		this.tree = new(tree, StringComparer.OrdinalIgnoreCase);
		Name = name;
		resolve = resolver;
	}

	public int Id { get; }
	public string Name { get; }
	public string[] FieldNames { get; }
	public IUnit this[string fieldName] => GetUnitValue(fieldName);
	public Function this[IDelayedUnit field] => (args) => GetUnitValue(field, args);
	public int Level { get; }

	private IUnit GetUnitValue(IDelayedUnit field, object[] param)
	{
		string fieldName = field.Name;
		string[] args = param.Select(o => o.ToString() ?? "").ToArray();
		var val = tree.GetValueOrDefault(fieldName, tree.GetValueOrDefault('{' + fieldName + '}', new NoValue(field.Level)));
		var delayed = val as IDelayedUnit;

		if (delayed == null)
			return new NoValue(field.Level);

		delayed = field;

		string path = delayed.ResolveUrl(args);
		var dic = new Dictionary<string, IUnit>(StringComparer.OrdinalIgnoreCase) { { "URL", new UnitValue(delayed.Id,delayed.Level, path) } };
		delayed = new DelayedUnit(delayed.Id,delayed.Level, delayed.Name, () => new UnitRecord(delayed.Id, delayed.Name, delayed.Level, dic, resolve));



		if (delayed.GetArity() > 0)
			return new NoValue(delayed.Level);

		return resolve(delayed);
	}

	private IUnit GetUnitValue(string fieldName)
	{
		var val = tree.GetValueOrDefault(fieldName, tree.GetValueOrDefault('{' + fieldName + '}', new NoValue(Level+1)));

		if (val is IDelayedUnit delayed && !ParserHelpers.IsDelayedValue(fieldName) && delayed.GetArity() == 0)
			val = resolve(delayed);

		return val;
	}
	public IUnitRecord Transform(IUnitRecord import, bool @override = false)
	{
		var tree = this.tree.ToDictionary(StringComparer.OrdinalIgnoreCase);

		foreach (var field in import.FieldNames)
			if (tree.TryGetValue(field, out var unit))
				if (@override || unit is IEmptyUnit)
					tree[field] = import[field];


		var result = new UnitRecord(Id, Name,Level, tree, resolve);

		return result;
	}


}
