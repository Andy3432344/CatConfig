using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CatConfig;
public static class Constructor
{

	private static NoValue? noValue = null;
	private static Dictionary<string, Dictionary<string, IDelayedProcessor>>
		processors = new(StringComparer.OrdinalIgnoreCase);

	public static bool RegisterProcessor(IDelayedProcessor processor)
	{
		if (!processors.TryGetValue(processor.ProtocolSchema, out var schemaProcs))
			schemaProcs = processors[processor.ProtocolSchema] = new(StringComparer.OrdinalIgnoreCase);

		return schemaProcs.TryAdd(processor.Name, processor);
	}


	private static IUnit GetDelayedUnitValue(IDelayedUnit delayed)
	{
		if (processors.TryGetValue(delayed.GetProtocolSchema(), out var procs))
			if (procs.TryGetValue(delayed.GetHostName(), out var proc))
				return proc.ResolveDelayedUnit(delayed);

		return noValue ??= new();
	}

	public static IUnit GetStructure(Ccl token, Parser parser) =>
		GetUnit(token.Id, token.StringValue, parser, token.Items);

	private static IUnit GetUnit(int id, string text, Parser parser, Dictionary<string, List<Ccl>> tree)
	{


		if (tree.Keys.Count == 0)
			return new UnitValue(id, text);
		else
			return GetComplexUnit(id, text, parser, tree);
	}

	private static IUnit GetComplexUnit(int id, string text, Parser parser, Dictionary<string, List<Ccl>> tree)
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
					IUnit unit;

					if (IsDelayed(item, out Ccl? delayed))
						unit = new DelayedUnit(delayed.Id, delayed.Level, item.Key[1..^1], GetDelayedRecord(delayed.StringValue), parser);
					else
						unit = GetUnit(v.Id, unitText, parser, v.Items);

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
			return rec.Values.First();//r5

		return new UnitRecord(id, text, rec, GetDelayedUnitValue);
	}

	private static string GetDelayedRecord(string content)
	{
		string ccl = "";
		char c = '\0';
		bool start = false;
		int i = 0;

		while (i < content.Length && (c = content[i]) != '}')
		{
			if (c != '{')
				ccl += content[i];

			i++;
		}

		if (c == '}')
			i++;

		return ccl + content[i..];
	}

	private static bool IsDelayed(KeyValuePair<string, List<Ccl>> candidate, [NotNullWhen(true)] out Ccl? delayedItem)
	{
		delayedItem = candidate.Value.FirstOrDefault();

		var delayed = (delayedItem != null &&
			candidate.Value.Count == 1 &&
			candidate.Key.Length > 1 &&
			candidate.Key[0] == '{' &&
			candidate.Key[^1] == '}');

		if (!delayed)
			delayedItem = null;

		return delayed;

	}
}



public record UnitValue(int Id, string Value) : IUnitValue;
public record UnitArray(int Id, IUnit[] Elements) : IUnitArray, IComplexUnit;
public record NoRecord : IUnitRecord
{

	private readonly NoValue noUnit = new();
	public IUnit this[string fieldName] => noUnit;

	public string Name => nameof(NoRecord);
	public string[] FieldNames => [];
	public int Id => -1;
}

public class UnitRecord : IUnitRecord, IComplexUnit
{
	private readonly NoValue noUnit;
	private readonly Dictionary<string, IUnit> tree;
	private readonly Func<IDelayedUnit, IUnit> resolver;
	public UnitRecord(int id, string name, Dictionary<string, IUnit> tree, Func<IDelayedUnit, IUnit> resolver)
	{
		Id = id;
		FieldNames = tree.Keys.ToArray();
		Name = name;
		this.resolver = resolver;
		this.tree = new(tree, StringComparer.OrdinalIgnoreCase);
		noUnit = new();
	}

	public int Id { get; }
	public string Name { get; }
	public string[] FieldNames { get; }
	public IUnit this[string fieldName] => GetUnitValue(fieldName);

	private IUnit GetUnitValue(string fieldName)
	{
		var val = tree.GetValueOrDefault(fieldName, noUnit);

		if (val is IDelayedUnit delayed)
			val = resolver(delayed);

		return val;
	}
}

public record UnitUrl(string Schema, string Key, string Host, string path);

public record NoValue(int Id = 0) : IUnit;
public record EmptyValue(int Id) : IUnit, IEmptyUnit;

public interface IDelayedProcessor
{
	string Name { get; }
	string ProtocolSchema { get; }

	IUnit ResolveDelayedUnit(IDelayedUnit delayed);
}

public interface IDelayedAccessor
{
	void DeliverRecord(int id, IUnitRecord hostRecord);
}

public interface IDelayedUnit : IComplexUnit
{
	void GetHostRecord(IDelayedAccessor accessor);
	string GetHostName();
	string GetProtocolSchema();
}


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
