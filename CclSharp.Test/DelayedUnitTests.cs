using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatConfig;

namespace CclSharp.Test
{
	public class DelayedUnitTests
	{

		static string url = "'app://Nunc/_app_/_doc_/{Donec}/$PRP.{Fusce}/'";

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
				Donec = 'ratione'
				Fusce = 'Praesent'
		""" + '\n';


		static string ipsum = "\tEtiam = _ -> true;";

		static string test = lorem + delayed + ipsum;

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

		private static IDelayedUnit? GetDelayedUnit(Parser parser)
		{
			var structure = parser.ParseContent("", test);

			var rec = structure as IUnitRecord;

			Assert.NotNull(rec);


			var value = rec["{Nunc}"];
			var unit = value as IDelayedUnit;
			return unit;
		}

		[Fact]
		public void TestLoadDelayedUnit()
		{

			var parser = Parser.FromContent("", "");

			var unit = GetDelayedUnit(parser);
			Assert.NotNull(unit);

			string ccl = unit.Value;

			ccl = ccl.Replace("{Nunc}", "Nunc");
			VerifyURL(parser, ccl);
		}

		[Fact]
		public void TestLoadDelayedUnitWhiteSpace()
		{

			var parser = Parser.FromContent("", "");

			var unit = GetDelayedUnit(parser);
			Assert.NotNull(unit);

			string ccl = unit.Value;

			ccl = ccl.Replace("{Nunc}", "Nunc");
			ccl = "\n\t\t\n\n" + ccl;

			VerifyURL(parser, ccl);
		}

		private static void VerifyURL(Parser parser, string ccl)
		{
			var placeHolder = parser.ParseContent("", ccl);


			var rec = placeHolder as IUnitRecord;
			Assert.NotNull(rec);

			var path = rec["URL"] as IUnitValue;
			Assert.NotNull(path);
			Assert.Equal(url, path.Value);
		}

	}
}

