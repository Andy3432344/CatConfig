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

    private UnitPath(string[] nodes, Range range)
    {
        this.nodes = nodes[range];
        path = string.Join('/', this.nodes);
    }

    public string this[int index]
    {
        get
        {
            return nodes[index];
        }
    }
    public UnitPath this[Range range]
    {
        get
        {
            return new(nodes, range);
        }
    }
    public int Length => nodes.Length;

    public static implicit operator string(UnitPath p) => p.path;
    public static implicit operator string[](UnitPath p) => p.nodes;
}