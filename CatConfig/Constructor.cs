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


    private static IUnit GetDelayedUnitValue(IDelayedUnit delayed)
    {
        if (delayed.GetArity() == 0)
            if (processors.TryGetValue(delayed.GetProtocolSchema(), out var procs))
                if (procs.TryGetValue(delayed.GetHostName(), out var proc))
                    return proc.ResolveDelayedUnit(delayed.Id, delayed.Name, delayed.GetPath());

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
                    {
                        string key = item.Key[1..^1];
                        unit = new DelayedUnit(delayed.Id, key, () => (parser.ParseContent("", GetDelayedRecord(delayed.StringValue)) as IUnitRecord) ?? NoRecord);
                    }
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
