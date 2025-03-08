namespace CatConfig.CclUnit;

public struct UnitPath
{
	private readonly string path;
	private readonly string[] nodes;

	public UnitPath(string path, char quote)
	{
		this.path = path;
		nodes = PathHelpers.GetAllNodes(path, quote);
	}
	public UnitPath()
	{
		nodes = [];
		path = "";
	}

	private UnitPath(string[] nodes, Range range)
	{
		if (range.Start.Value < nodes.Length && range.End.Value < nodes.Length)
			this.nodes = nodes[range];

		path = string.Join('/', (this.nodes ?? []));
	}

	public string this[int index]
	{
		get
		{
			if (nodes == null ||index  >= nodes.Length)
				return "";

			return nodes[index];
		}
	}
	public UnitPath this[Range range]
	{
		get
		{
			if (nodes == null)
				return new();

			return new(nodes, range);
		}
	}
	public int Length => nodes?.Length ?? 0;

	public static implicit operator string(UnitPath p) => p.path;
	public static implicit operator string[](UnitPath p) => p.nodes;
}