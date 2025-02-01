using System.Reflection.Metadata;
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
    public void TestQuotedValue()
    {
        string meta = """
    meta =
    	QuoteLiteral=  ' 
    	Indent= '\t'
    	IndentStep = 1
    	Delimiter = '='\n
    """;

        string ccl = "Key = -Value-\nKey2= -'Value'-";
        var p = Parser.FromContent("", meta + ccl);

        RunTest(ccl, "-'Value'-", parser);
        RunTest(ccl, "Value", p);
    }

    private static void RunTest(string ccl, string value, Parser p)
    {
        var record = p.ParseContent("", ccl) as IUnitRecord;
        Assert.NotNull(record);

        var key = record["Key"] as IUnitValue;
        var key2 = record["key2"] as IUnitValue;

        Assert.NotNull(key);
        Assert.NotNull(key2);

        Assert.Equal("-Value-", key.Value);
        Assert.Equal(value, key2.Value);
    }

    [Fact]
    public void SkippedLines()
    {
        string ccl = $""" 

            Key= 
            {'\t'}=Value1
            
            {'\t'}=Value2

            """;


        var values = parser.ParseContent(nameof(SkippedLines), ccl);

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
        string ccl = "FOO=BAR\nKEY=\nFIZZ=BUZ";

        var keyOnly = parser.ParseContent("", ccl);

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

        var valueOnly = parser.ParseContent("", ccl);

        var valueRecord = valueOnly as IUnitRecord;

        Assert.NotNull(valueRecord);

        var value = valueRecord[""];

        var val = value as IUnitValue;

        Assert.NotNull(val);

        Assert.Equal("VALUE", val.Value);
    }
    [Fact]
    public void ArrayOnly()
    {
        string ccl = "";
        for (int i = 0; i < 9; i++)
        {

            ccl += $"=VALUE{i + 1}\n";
        }

        var valueOnly = parser.ParseContent("", ccl);

        var valueRecord = valueOnly as IUnitArray;

        Assert.NotNull(valueRecord);

    }
    [Fact]
    public void DuplicateKeys()
    {
        string ccl = """
            Key = Value1
            Key = Value2
            """;

        var dups = parser.ParseContent("", ccl);
        var arr = dups as IUnitArray;

        Assert.NotNull(arr);

        var val1 = arr.Elements[0] as IUnitValue;
        var val2 = arr.Elements[1] as IUnitValue;

        Assert.NotNull(val1);
        Assert.NotNull(val2);

        Assert.Equal("Value1", val1.Value);
        Assert.Equal("Value2", val2.Value);

    }

    [Fact]
    public void KeyDoesNotSpanLines()
    {
        string ccl = "K\ney = Value1";

        var spn = parser.ParseContent("", ccl);
        var record = spn as IUnitRecord;
        Assert.NotNull(record);
        var bad = record["k\ney"];

        Assert.IsAssignableFrom<NoValue>(bad);

    }
}