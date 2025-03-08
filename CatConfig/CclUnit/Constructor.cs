
using System.Diagnostics.CodeAnalysis;
using CatConfig;
using CatConfig.CclParser;
namespace CatConfig.CclUnit;
public static class Constructor
{
	internal static NoRecord NoRecord => noRecord ??= new();
	private static NoRecord? noRecord = null;
	private static NoValue? noValue = null;
	private static Dictionary<string, Dictionary<string, IDelayedProcessor>>
		processors = new(StringComparer.OrdinalIgnoreCase);

	public static bool RegisterProcessor(IDelayedProcessor processor)
	{
		if (!processors.TryGetValue(processor.ProtocolSchema, out var schemaProcs))
			schemaProcs = processors[processor.ProtocolSchema] = new(StringComparer.OrdinalIgnoreCase);

		return schemaProcs.TryAdd(processor.Name, processor);
	}


	public static IUnit GetStructure(Ccl token, Parser parser) =>
		GetUnit(token.Id, token.StringValue, parser, token.Items);

	private static IUnit GetUnit(int id, string text, Parser parser, Dictionary<string, List<Ccl>> tree)
	{
		int level = tree.Any() ?
		(tree.Values.First()?.First()?.Level ?? 0) :
		0;

		if (tree.Keys.Count == 0)
			return new UnitValue(id, level, text);
		else
			return GetComplexUnit(id, text, level, parser, tree);
	}
	private static IUnit GetUnit(Ccl node, string text, Parser parser)
	{
		var tree = node.Items;
		var id = node.Id;

		if (tree.Keys.Count == 0)
			return new UnitValue(id, node.Level, text);
		else
			return GetComplexUnit(id, text, node.Level, parser, tree);
	}
	private static IUnit GetComplexUnit(int id, string text, int level, Parser parser, Dictionary<string, List<Ccl>> tree)
	{
		var allUnits = GetUnits(text, parser, tree, out var keys);

		var units = ConsolidateUnits(id, level, allUnits, keys);

		if (string.IsNullOrEmpty(text) && units.Count == 1 && units.Values.First() is IComplexUnit)
			return units.Values.First();//r5

		AddEmptyUnitForMissingPlaceHolders(units);//r12

		return new UnitRecord(id, text, level, units, d => GetDelayedUnitValue(d, parser.QuoteExpansion));
	}


	private static Dictionary<string, IUnit> ConsolidateUnits(int id, int level, Dictionary<string, List<IUnit>> units, int keys)
	{
		Dictionary<string, IUnit> namedUnits = new();

		foreach (var unit in units)
		{
			if (unit.Value.Count == 0)
				namedUnits.Add(unit.Key, new EmptyValue(id, level));
			if (unit.Value.Count == 1 && keys > 0)
				namedUnits.Add(unit.Key, unit.Value.First());
			else if (unit.Value.Count > 0)
			{
				var arr = new UnitArray(id, level, unit.Value.ToArray());
				namedUnits.Add(unit.Key, arr);
			}

		}

		return namedUnits;
	}
	private static Dictionary<string, List<IUnit>> GetUnits(string text, Parser parser, Dictionary<string, List<Ccl>> tree, out int keyLength)
	{
		Dictionary<string, List<IUnit>> units = new();
		keyLength = 0;
		foreach (var item in tree)
		{
			List<IUnit> values = new();

			foreach (var v in item.Value)
			{
				keyLength += v.KeyLength;
				string unitText = v.StringValue;
				if (!string.IsNullOrEmpty(unitText))
				{
					IUnit unit;
					if (IsDelayed(item, out Ccl? delayed))
						unit = new DelayedUnit(delayed.Id, delayed.Level, item.Key[1..^1], () => parser.ParseContent("", GetDelayedRecord(delayed.StringValue)) as IUnitRecord ?? NoRecord);
					else
						unit = GetUnit(v, unitText, parser);

					//unit = GetUnit(v.Id, unitText, parser, v.Items);

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

		return units;
	}


	private static void AddEmptyUnitForMissingPlaceHolders(Dictionary<string, IUnit> units)
	{
		if (units.TryGetValue("URL", out var url))
			if (url is IUnitValue unit)
				foreach (var field in GetFields(unit.Value))
					if (!(units.ContainsKey(field) || units.ContainsKey('{' + field + '}')))
						units.Add(field, new EmptyValue(unit.Id, unit.Level));
	}
	private static IUnit GetDelayedUnitValue(IDelayedUnit delayed, char qtExpand)
	{
		if (delayed.GetArity() == 0)
			if (processors.TryGetValue(delayed.GetProtocolSchema(), out var procs))
				if (procs.TryGetValue(delayed.GetHostName(), out var proc))
					return proc.ResolveDelayedUnit(delayed, delayed.Name, delayed.GetPath(qtExpand));

		return new NoValue(delayed.Level);
	}

	private static string[] GetFields(string url)
	{
		int i = 0;
		int phase = 0;
		string current = "";
		List<string> fields = new();

		while (i < url.Length)
		{
			char c = url[i];
			i++;

			if (phase == 0 && c == '{')
			{
				phase = 1;
				continue;
			}
			else
			if (phase == 1 && c != '}')
			{
				current += c;
				continue;
			}
			else if (phase == 1)
			{
				//here: phase == 1 && c == '}'
				phase = 0;

				fields.Add(current);
				current = "";
			}
		}
		return fields.ToArray();
	}

	private static string GetDelayedRecord(string content)
	{
		string ccl = "";
		char c = '\0';
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

		var delayed = delayedItem != null &&
			candidate.Value.Count == 1 &&
			candidate.Key.Length > 1 &&
			candidate.Key[0] == '{' &&
			candidate.Key[^1] == '}';

		if (!delayed)
			delayedItem = null;

		return delayed;

	}
}
