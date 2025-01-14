using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatConfig;
public static class Constructor
{
	public static IUnit GetStructure(Ccl token) =>
		GetUnit(token.Id, token.StringValue, token.Items);



	private static IUnit GetUnit(int id, string text, Dictionary<string, List<Ccl>> tree)
	{
		if (tree.Keys.Count == 0)
			return new UnitValue(id, text);
		else
			return GetComplexUnit(id, text, tree);
	}

	private static IUnit GetComplexUnit(int id, string text, Dictionary<string, List<Ccl>> tree)
	{
		Dictionary<string, List<IUnit>> units = new();

		foreach (var item in tree)
		{
			List<IUnit> values = new();

			foreach (var v in item.Value)
			{
				string unitText = v.StringValue;
				if (!string.IsNullOrEmpty(unitText))
				{
					var unit = GetUnit(v.Id, unitText, v.Items);

					if (unit is IUnitRecord uRec)
						if (uRec.FieldNames.Length == 1)
							if (uRec.FieldNames[0].Equals(uRec.Name, StringComparison.OrdinalIgnoreCase))
								unit = uRec[uRec.FieldNames[0]];

					values.Add(unit);
				}
			}

			if (string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(text))
				units.Add(text, values);
			else
				units.Add(item.Key, values);

		}


		Dictionary<string, IUnit> rec = new();

		foreach (var unit in units)
		{

			if (unit.Value.Count == 0)
				rec.Add(unit.Key, new EmptyValue(id));
			if (unit.Value.Count == 1)
				rec.Add(unit.Key, unit.Value.First());
			else if (unit.Value.Count > 0)
			{
				var arr = new UnitArray(id, unit.Value.ToArray());
				rec.Add(unit.Key, arr);
			}

		}

		if (string.IsNullOrEmpty(text) && rec.Count == 1 && rec.Values.First() is IComplexUnit)
			return rec.Values.First();

		return new UnitRecord(id, text, rec);
	}
}

public record UnitValue(int Id, string Value) : IUnitValue;
public record UnitArray(int Id, IUnit[] Elements) : IUnitArray, IComplexUnit;


public class UnitRecord : IUnitRecord, IComplexUnit
{
	private readonly NoValue noUnit;
	private readonly Dictionary<string, IUnit> tree;

	public UnitRecord(int id, string name, Dictionary<string, IUnit> tree)
	{
		Id = id;
		FieldNames = tree.Keys.ToArray();
		Name = name;
		this.tree = new(tree, StringComparer.OrdinalIgnoreCase);
		noUnit = new();
	}

	public int Id { get; }
	public string Name { get; }
	public string[] FieldNames { get; }
	public IUnit this[string fieldName] => tree.GetValueOrDefault(fieldName, noUnit);
}

public record NoValue(int Id = 0) : IUnit;
public record EmptyValue(int Id) : IUnit, IEmptyUnit;

public interface IComplexUnit : IUnit;
public interface IEmptyUnit : IUnit;
public interface IUnit
{
	int Id { get; }
}

public interface IUnitValue : IUnit
{
	string Value { get; }
}


public interface IUnitArray : IUnit
{
	IUnit[] Elements { get; }
}


public interface IUnitRecord : IUnit
{
	string Name { get; }
	string[] FieldNames { get; }
	IUnit this[string fieldName] { get; }
}
