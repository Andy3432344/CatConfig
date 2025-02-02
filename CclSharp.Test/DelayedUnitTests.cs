using CatConfig;
using CatConfig.CclParser;
using CatConfig.CclUnit;
using static CclSharp.Test.TestHelpers;

namespace CclSharp.Test
{

    public class DelayedUnitTests
    {
        public DelayedUnitTests()
        {
            Constructor.RegisterProcessor(new TestSumIntegerUnitProcessor());
            Constructor.RegisterProcessor(new TestOrderQuantityLookupProcessor());
        }

        static string url = "test://Sum/{x}+{y}";
        static string lorem =
        """
		Interdum  =
			Vestibulum  = 'non'
			Neque = '*.*'
			Maecenas  = true
			Morbi = 'enim'
		""";

        static string delayed(string x, string y) => '\n' +
        $$"""
			{Sum} = 
				URL = {{url}}
				x = {{x}}
				y = {{y}}
		""" + '\n';
        static string ipsum = "\tEtiam = true";


        static string test(string x, string y) => lorem + delayed(x, y) + ipsum;

        [Fact]
        public void TestDelayedUnit()
        {
            var test = DelayedUnitTests.test("5", "10");
            var parser = Parser.FromContent("", test);
            var structure = parser.ParseContent("", test);

            var rec = structure as IUnitRecord;
            Assert.NotNull(rec);

            var value = rec["Sum"];
            var unit = value as IUnitValue;

            Assert.NotNull(unit);

            Assert.Equal("15", unit.Value);

        }


        [Fact]
        public void TestDelayedUnitParameter()
        {
            var test = DelayedUnitTests.test("", "");

            var parser = Parser.FromContent("", test);
            var structure = parser.ParseContent("", test);
            var rec = structure as IUnitRecord;

            Assert.NotNull(rec);

            var wait = rec["Sum"] as IDelayedUnit;
            Assert.NotNull(wait);

            var unit = rec[wait](7, 4) as IUnitValue;
            Assert.NotNull(unit);

            Assert.Equal("11", unit.Value);

        }

        [Fact]
        public void TestNestedDelayedUnit()
        {
            var meta = GetMeta('\t', 1, '=', '\'', '"');
            string nestedDelayed = $$"""
			NestedTest =
				{CalculatedValue} = 
					URL = test://Sum/{OrderQuantity}+{Replacements}
					{OrderQuantity} = 
						URL = test://OrderProcessor/{OrderNumber}/Quantity
						OrderNumber =
					Replacements = 3
			""";

            string ccl = meta + '\n' + nestedDelayed;
            var p = Parser.FromContent("", ccl);
            var rec = p.ParseContent("", ccl) as IUnitRecord;

            Assert.NotNull(rec);

            Assert.Single(rec.FieldNames);

            var wait = rec["CalculatedValue"] as IDelayedUnit;
            Assert.NotNull(wait);

            var unit = rec[wait]("JM-323L") as IUnitValue;
            Assert.NotNull(unit);

            Assert.Equal("10", unit.Value);
        }

        [Fact]
        public void TestExpansionQuotedDelayedUnit()
        {
            var meta = GetMeta('\t', 1, '=', '\'', '"');
            var ccl = meta + '\n' +
            """
            Order =	
            	{OrderQuantity} = 
            		URL = test://OrderProcessor/"{OrderNumber}"/Quantity
            		OrderNumber =
            """;

            var p = Parser.FromContent("", ccl);

            Assert.Equal('"', p.QuoteExpansion);
            var rec = p.ParseContent("", ccl) as IUnitRecord;
            Assert.NotNull(rec);

            Assert.Single(rec.FieldNames);

            var wait = rec["OrderQuantity"] as IDelayedUnit;
            Assert.NotNull(wait);

            var unit = rec[wait]("HN/787K") as IUnitValue;
            Assert.NotNull(unit);

            Assert.Equal("12", unit.Value);
        }
           
        [Fact]
        public void MissingPlaceholderFieldsAreAddedAutomatically()
        {
            var test = "File=\n\t{FileSize}=\n\t\tURL = test://Sum/5+{y}";

            var parser = Parser.FromContent("", test);
            var structure = parser.ParseContent("", test);

            var rec = structure as IUnitRecord;
            Assert.NotNull(rec);

            var wait = rec["FileSize"] as IDelayedUnit;
            Assert.NotNull(wait);

            var unit = rec[wait](15) as IUnitValue;
            Assert.NotNull(unit);

            Assert.Equal("20", unit.Value);
        }



    }
}

