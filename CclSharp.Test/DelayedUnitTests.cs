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


		static string nestedDelayedUnitTest(string method) => $$$"""
			NestedTest =
				{CalculatedValue} = 
					URL = test://Test/{Path}/$PRP/{Arg}
					{Path} = 
						URL = lang://CSharp/({{{GetMethod(method)}}})"
						FilePath = ''
					Arg = 'testing'
			""";


		static string GetMethod(string method)
		{
			return $$$"""
				MethodBlock(({expression}));
				string MethodBlock(string parameter = {ParameterExpression}.ToString())
				{
					{{{method}}}
				}
				""";
		}


		[Fact]
		public void TestNestedDelayedUnit()
		{
			var t = nestedDelayedUnitTest("""
				var directory = Path.GetDirectoryName(parameter);
				var fileName = Path.GetFileNameWithoutExtension(parameter);

				if (directory != null)
				{
					var dir = Directory.GetParent(directory);
					if (dir != null)
					{
						int i = 0;
						while (i < fileName.Length && !char.IsDigit(fileName[i]))
							i++;

						int start = i;
						int dash = 0;

						while (i < fileName.Length && (char.IsDigit(fileName[i]) || fileName[i]=='-' && ++dash==1))
							i++;

						int end = i;

						string drawingNumber ="H-" + fileName[start..end] + ".SLDDRW";
						return Path.Combine(dir.FullName, drawingNumber);

					}
				}
				return "";
				""");
			Assert.NotNull(t);

			var p = Parser.FromContent("", t);
			var s = p.ParseContent("", t);

			if (s is IUnitRecord rec)
			{
				Assert.Single(rec.FieldNames);

				var wait = rec["{CalculatedValue}"] as IDelayedUnit;
				Assert.NotNull(wait);

				var unit = rec[wait]("testPath","TestArg" ) as IUnitValue;
				Assert.NotNull(unit);




			}
		}
	}
}

