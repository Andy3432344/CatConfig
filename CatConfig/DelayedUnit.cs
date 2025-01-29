using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CatConfig;

public class DelayedUnit : IDelayedUnit
{
	private readonly string schema = "";
	private readonly string host = "";
	private readonly string path = "";

	public DelayedUnit(int id, string key, Func<IUnitRecord> getRecord)
	{
		Id = id;
		Name = key;
		this.getRecord = getRecord;
		(schema, host, path) = InterpolationHelpers.GetPathParts(GetUrlPath());
	}

	public int GetArity()
	{
		var rec = GetRecord();
		var fields = rec.FieldNames.Where(f => !f.Equals("url", StringComparison.OrdinalIgnoreCase)).ToArray();

		int unResolved = 0;


		foreach (var f in fields)
		{
			var field = rec[f];
			if (field is IDelayedUnit wait)
				unResolved += wait.GetArity();
			else if (field is IEmptyUnit)
				unResolved++;
		}

		return unResolved;
	}
	public int Id { get; }
	public string Name { get; }

	private string GetUrlPath()
	{
		var delayed = GetRecord();

		var url = delayed["URL"];
		string path = "";
		if (url is IUnitValue unit)
			path = unit.Value;

		return path;
	}

	protected IUnitRecord GetRecord() => unit ??= getRecord();
	private IUnitRecord? unit = null;
	private readonly Func<IUnitRecord> getRecord;
	public string GetProtocolSchema() =>
		Resolve(schema, []);

	public string GetHostName() =>
		Resolve(host, []);

	public string GetPath() =>
		Resolve(path, []);

	public string ResolveUrl(params string[] parameters) =>
		Resolve(GetUrlPath(), parameters);

	private string Resolve(string url, string[] parameters)
	{
		var rec = GetRecord();
		int index = 0;
		int i = 0;
		int phase = 0;
		string result = "";
		string current = "";

		while (i < url.Length)
		{
			char c = url[i];
			i++;

			if (phase == 0)
			{
				if (c == '{')
					phase = 1;
				else
					result += c;

				continue;
			}
			else
			if (c != '}')
			{
				current += c;
				continue;
			}

			//here: phase == 1 && c == '}'
			phase = 0;

			var value = InterpolationHelpers.ResolveWithRecord(rec[current], rec, current);
			if (value == '{' + current + '}')
				value = InterpolationHelpers.ResolveWithParameters(current, parameters, rec, ref index);

			result += value ?? current;
			if (value != null)
				index++;

			current = "";
		}

		return result;
	}

}
