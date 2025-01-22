using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatConfig;

namespace CclSharp.Test
{

	public class DelayedUnitTests
	{
		public DelayedUnitTests()
		{
			Constructor.RegisterProcessor(new TestUnitProcessor());
		}

		static string url = "test://Test/{x}+{y}";
		static string lorem =
		"""
		Interdum  =
			Vestibulum  = 'non'
			Neque = '*.*'
			Maecenas  = true
			Morbi = 'enim'
		""";

		static string delayed = '\n' +
		$$"""
			{Sum} = 
				URL = {{url}}
				x = 10
				y = 5
		""" + '\n';


		static string ipsum = "\tEtiam = true";
		static string test = lorem + delayed + ipsum;

		[Fact]
		public void TestDelayedUnit()
		{

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

			var parser = Parser.FromContent("", test);
			var structure = parser.ParseContent("", test);
			var rec = structure as UnitRecord;

			Assert.NotNull(rec);

			var wait = rec["{Sum}"] as IDelayedUnit;
			Assert.NotNull(wait);

			var unit = rec[wait](7, 4) as IUnitValue;
			Assert.NotNull(unit);

			Assert.Equal("11", unit.Value);

		}
	}
}

