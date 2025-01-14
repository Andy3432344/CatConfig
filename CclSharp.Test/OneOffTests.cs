using CatConfig;
using Newtonsoft.Json.Linq;

namespace CclSharp.Test;

public class OneOffTests
{
	private Parser parser = Parser.FromFile("");

	[Fact]
    public void BlankValue()
    {
        TestBlankValue("");
        TestBlankValue("\t");
        TestBlankValue("\t\t\t");
        TestBlankValue("\n");
        TestBlankValue("\t\t\n");
        TestBlankValue("\n\n");
        TestBlankValue("\t\t\n\t\t\n\t\t");
    }

    private void TestBlankValue(string ccl)
    {
        var empty = parser.ParseContent("", ccl);

        Assert.Equal(0, empty.Id);
    }

    [Fact]
    public void SkippedLines()
    {
        string ccl = $""" 

            Key= 
            {'\t'}=Value1
            
            {'\t'}=Value2

            """;


        var newLines = parser.ParseContent(nameof(SkippedLines), ccl);
        var values = Constructor.GetStructure(newLines);

        var recordValue = values as IUnitRecord;

        Assert.NotNull(recordValue);

        var arr = recordValue["Key"] as IUnitArray;
        Assert.NotNull(arr);

        var val1 = arr.Elements[0] as IUnitValue;
        var val2 = arr.Elements[1] as IUnitValue;

        Assert.NotNull(val1);
        Assert.NotNull(val2);

        Assert.Equal("Value1", val1.Value);
        Assert.Equal("Value2", val2.Value);

    }

        [Fact]
    public void KeyOnly()
    {
        string ccl = "KEY=";

        var emptyValue = parser.ParseContent("", ccl);

        Assert.Equal(1, emptyValue.Id);

        var keyOnly = Constructor.GetStructure(emptyValue);
        var keyRecord = keyOnly as IUnitRecord;

        Assert.NotNull(keyRecord);

        var keyValue = keyRecord["Key"];

        var empty = keyValue as IEmptyUnit;

        Assert.NotNull(empty);

        Assert.Equal(1, empty.Id);
    }
    [Fact]
    public void ValueOnly()
    {
        string ccl = "=VALUE";

        var emptyKey = parser.ParseContent("", ccl);

        Assert.Equal(1, emptyKey.Id);
        var valueOnly = Constructor.GetStructure(emptyKey);
        var valueRecord = valueOnly as IUnitRecord;

        Assert.NotNull(valueRecord);

        var value = valueRecord[""];

        var val = value as IUnitValue;

        Assert.NotNull(val);

        Assert.Equal("VALUE", val.Value);
    }

    [Fact]
    public void DuplicateKeys()
    {
        string ccl = """
            Key = Value1
            Key = Value2
            """;

        var dupKeys = parser.ParseContent("", ccl);
        var dups = Constructor.GetStructure(dupKeys);

        var arr = dups as IUnitArray;

        Assert.NotNull(arr);

        var val1 = arr.Elements[0] as IUnitValue;
        var val2 = arr.Elements[1] as IUnitValue;

        Assert.NotNull(val1);
        Assert.NotNull(val2);

        Assert.Equal("Value1", val1.Value);
        Assert.Equal("Value2", val2.Value);

    }
}