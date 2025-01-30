using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CatConfig;

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

        static string nestedDelayed = $$"""
			NestedTest =
				{CalculatedValue} = 
					URL = test://Sum/{OrderQuantity}+{Replacements}
					{OrderQuantity} = 
						URL = test://OrderProcessor/{OrderNumber}/Quantity
						OrderNumber =
					Replacements = 3
			""";

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

            var wait = rec["{Sum}"] as IDelayedUnit;
            Assert.NotNull(wait);

            var unit = rec[wait](7, 4) as IUnitValue;
            Assert.NotNull(unit);

            Assert.Equal("11", unit.Value);

        }


        [Fact]
        public void TestNestedDelayedUnit()
        {
            var p = Parser.FromContent("", nestedDelayed);
            var rec = p.ParseContent("", nestedDelayed) as IUnitRecord;

            Assert.NotNull(rec);

            Assert.Single(rec.FieldNames);

            var wait = rec["CalculatedValue"] as IDelayedUnit;
            Assert.NotNull(wait);

            var unit = rec[wait]("JM-323L") as IUnitValue;
            Assert.NotNull(unit);

            Assert.Equal("10", unit.Value);

        }
    }
}

