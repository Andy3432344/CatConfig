namespace CatConfig;

public class UnitRecord : IUnitRecord
{
	private readonly NoValue noUnit;
	private readonly Dictionary<string, IUnit> tree;
	private readonly Func<IDelayedUnit, IUnit> resolve;

	public UnitRecord(int id, string name, Dictionary<string, IUnit> tree, Func<IDelayedUnit, IUnit> resolver)
	{
		Id = id;
		FieldNames = tree.Keys.ToArray();
		this.tree = new(tree, StringComparer.OrdinalIgnoreCase);
		Name = name;
		this.resolve = resolver;
		noUnit = new();
	}

	public int Id { get; }
	public string Name { get; }
	public string[] FieldNames { get; }
	public IUnit this[string fieldName] => GetUnitValue(fieldName);
	public Function this[IDelayedUnit field] => (args) => GetUnitValue(field, args);

	private IUnit GetUnitValue(IDelayedUnit field, object[] param)
	{
		string fieldName = field.Name;
		string[] args = param.Select(o => o.ToString() ?? "").ToArray();
		var val = tree.GetValueOrDefault(fieldName, tree.GetValueOrDefault('{' + fieldName + '}', noUnit));
		var delayed = val as IDelayedUnit;

		if (delayed == null)
			return noUnit;

		if (delayed.GetArity() > 0 || param.Length > 0)
		{
			string path = delayed.ResolveUrl(args);
			var dic = new Dictionary<string, IUnit>(StringComparer.OrdinalIgnoreCase) { { "URL", new UnitValue(delayed.Id, path) } };
			delayed = new DelayedUnit(delayed.Id, delayed.Name, () => new UnitRecord(delayed.Id, delayed.Name, dic, resolve));
		}


		if (delayed.GetArity() > 0)
			return noUnit;

		return resolve(delayed);
	}

	private IUnit GetUnitValue(string fieldName)
	{
		var val = tree.GetValueOrDefault(fieldName, tree.GetValueOrDefault('{' + fieldName + '}', noUnit));

		if (val is IDelayedUnit delayed && !ParserHelpers.IsDelayedValue(fieldName) && delayed.GetArity() == 0)
			val = resolve(delayed);

		return val;
	}


}
