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
		Name = name;
		this.resolve = resolver;
		this.tree = new(tree, StringComparer.OrdinalIgnoreCase);
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


		Func<int, string, string, IUnitRecord> resolver = (id, name, path)
			=> new UnitRecord(
				id,
				name,
				new Dictionary<string, IUnit>() { { "URL", new UnitValue(id, path) } },
				resolve);

		delayed = delayed.ResolveUrl(resolver, args);

		//if (fieldName.FirstOrDefault() != '{' && fieldName.LastOrDefault() != '}')
		return resolve(delayed);


	}

	private IUnit GetUnitValue(string fieldName)
	{
		var val = tree.GetValueOrDefault(fieldName, tree.GetValueOrDefault('{' + fieldName + '}', noUnit));

		if (val is IDelayedUnit delayed && fieldName.FirstOrDefault() != '{' && fieldName.LastOrDefault() != '}')
			val = resolve(delayed);

		return val;
	}

	public IUnitRecord Await(IDelayedUnit delayed, string urlPath)
	{
		var url = new UnitValue(delayed.Id, urlPath);
		var dic = new Dictionary<string, IUnit>() { { "URL", url } };
		return new UnitRecord(delayed.Id, delayed.Name, dic, resolve);
	}


	public Func<int, string, IUnitRecord> Await(int id, string urlPath)
	{
		var url = new UnitValue(id, urlPath);
		var dic = new Dictionary<string, IUnit>() { { "URL", url } };
		return (i, s) => new UnitRecord(i, s, dic, resolve);
	}

}
