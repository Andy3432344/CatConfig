namespace CatConfig.CclParser;

public class Ccl(int index, int level, string text)
{
    public int Id => index + 1;
    public int Level => level;
    public string StringValue { get; init; } = text;
    public Dictionary<string, List<Ccl>> Items { get; } = new();
}
