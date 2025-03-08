namespace CatConfig.CclParser;

public class Ccl(int index, int level, string text, int keyLength)
{
	public int Id { get; } = index + 1;
	public int Level { get; } = level;
	public string StringValue { get; init; } = text;
	public int KeyLength { get; } = keyLength;
	public Dictionary<string, List<Ccl>> Items { get; } = new();



}
