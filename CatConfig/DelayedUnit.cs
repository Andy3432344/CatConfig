namespace CatConfig;

public class DelayedUnit : IDelayedUnit
{
	private readonly string schema = "";
	private readonly string host = "";
	private readonly string path = "";

	public int Id { get; }
	public string Name { get; }

	public DelayedUnit(int id, string key, Func<IUnitRecord> getRecord)
	{
		Id = id;
		Name = key;
		this.getRecord = getRecord;
		string fullPath = GetUrlPath();
		int phase = 0;

		int i = 0;
		while (i < fullPath.Length)
		{
			char c = fullPath[i];

			switch (phase)
			{
				case 0://schema
					if (c == ':')
						phase = 1;
					else
						schema += c;
					break;
				case 1: // "//"
					if (c == '/')
						phase = 2;
					break;
				case 2: // "//"
					if (c == '/')
						phase = 3;
					break;
				case 3: //host
					if (c == '/')
						phase = 4;
					else
						host += c;
					break;
				case 4: //path
					path += c;
					break;
			}
			i++;
		}
	}

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



	public string GetProtocolSchema()
	{
			return Interpolate(schema);
	}

	public string GetHostName()
	{
			return Interpolate(host);
	}

	public string GetPath()
	{
			return Interpolate(path);
	}

	public IDelayedUnit ResolveUrl(Func<int, string, string, IUnitRecord> resolver, string[] fields)
	{
		var index = 0;
		string schemaName = GetFormattedString(schema, fields, ref index);
		string hostName = GetFormattedString(host, fields, ref index);
		string requestPath = GetFormattedString(path, fields, ref index);

		string fullPath = $"{schemaName}://{hostName}/{requestPath}";

		return new DelayedUnit(Id, Name, () => resolver(Id, Name, fullPath));

	}


	private string GetFormattedString(string text, string[] fields, ref int index)
	{
		string result = "";
		int phase = 0;
		int i = 0;

		while (i < text.Length)
		{
			char c = text[i];
			if (phase == 0)
			{
				if (c == '{')
					phase = 1;
				else
					result += c;
			}
			else
			{
				if (c == '}')
				{
					phase = 0;
					result += fields[index];
					index++;
				}
			}
			i++;
		}

		return result;
	}



	private string Interpolate(string value)
	{
		var rec = GetRecord();
		string resolved = "";
		string current = "";
		int phase = 0;
		int i = 0;

		while (i < value.Length)
		{
			char c = value[i];
			switch (phase)
			{
				case 0:
					if (c == '{')
						phase = 1;
					else
						resolved += c;
					break;
				case 1:
					if (c == '}')
					{
						phase = 0;
						resolved += (rec[current] as IUnitValue)?.Value ?? "";
						current = "";
					}
					else
						current += c;
					break;

			}
			i++;
		}

		return resolved;

	}
}
