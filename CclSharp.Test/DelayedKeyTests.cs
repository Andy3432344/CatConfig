using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatConfig;

namespace CclSharp.Test
{
	public class DelayedKeyTests
	{

		static string lorem =
		"""
		Interdum  =
			Vestibulum  = 'non'
			Neque = '*.*'
			Maecenas  = true
			Morbi = 'enim'
		""";

		static string ipsum = "\tEtiam = _ -> true;";


		static string delayed = '\n' +
		"""
			{Nunc} = 
				URL = 'app://Nunc/_app_/_doc_/{Donec}/$PRP.{Fusce}/'
				Donec = 'ratione'
				Fusce = 'Praesent'
		""" + '\n';


		static string test = lorem +  delayed  + ipsum;

		[Fact]
		public void TestDelayedUnit()
		{

			var parser = Parser.FromContent("", "");

			var unit = GetDelayedUnit(parser);
			Assert.NotNull(unit);

			string expected = TestHelpers.RemoveAllWhiteSpace(delayed, parser.Indent);
			string actual = TestHelpers.RemoveAllWhiteSpace(unit.Value, parser.Indent);

			Assert.Equal(expected, actual);
		}


		[Fact]
		public void TestLoadDelayedUnit()
		{

			var parser = Parser.FromContent("", "");

			var unit = GetDelayedUnit(parser);
			Assert.NotNull(unit);



		}

		private static IDelayedUnit GetDelayedUnit(Parser parser)
		{
			var structure = parser.ParseContent("", test);

			var rec = structure as IUnitRecord;

			Assert.NotNull(rec);


			var value = rec["{Nunc}"];
			var unit = value as IDelayedUnit;
			return unit;
		}
	}
}

