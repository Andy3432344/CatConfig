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
		static string url = "test://Test/{x}/{y}";

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
			{Nunc} = 
				URL = {{url}}
				x = 10
				y = 5
		""" + '\n';


		static string ipsum = "\tEtiam = _ -> true;";

		static string test = lorem + delayed + ipsum;

		[Fact]
		public void TestDelayedUnit()
		{

			var parser = Parser.FromContent("", "");
var structure = parser.ParseContent("", test);

			var rec = structure as IUnitRecord;

			Assert.NotNull(rec);


			var value = rec["{Nunc}"];
			var unit = value as IUnitValue;
		  
			Assert.NotNull(unit);

			Assert.Equal("15", unit.Value);

		}


	}
}

