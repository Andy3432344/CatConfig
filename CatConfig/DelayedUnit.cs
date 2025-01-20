namespace CatConfig;

public class DelayedUnit : IDelayedUnit
{
	private readonly int level;
	private readonly string schema = "";
	private readonly string key = "";
	private readonly string host = "";
	private readonly string path = "";

	public int Id { get; }

	public void GetHostRecord(IDelayedAccessor accessor) =>
		accessor.DeliverRecord(Id, GetRecord());


	public DelayedUnit(int id, int level, string key, string value, Parser parser)
	{
		Id = id;
		this.level = level;
		this.value = value;
		this.parser = parser;
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

	protected IUnitRecord GetRecord() => unit ??= (parser.ParseContent(key, value) as IUnitRecord ?? NoRecord);
	private IUnitRecord? unit = null;
	private readonly Parser parser;
	private readonly string value;
	protected NoRecord NoRecord => noRecord ??= new();
	private NoRecord? noRecord = null;


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
						resolved += rec[current];
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
