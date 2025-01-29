using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using CatConfig;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit.Abstractions;

namespace CclSharp.Test;

public class NestedValueTests
{
	private readonly ITestOutputHelper output;

	public NestedValueTests(ITestOutputHelper output)
	{
		this.output = output;
	}
	private Func<char, int, char, string> meta = (char i, int s, char d) => $"""
		meta =
			Indent= '{i}'
			IndentStep = {s}
			Delimiter = '{d}'
		""";

	private string MultiLineValue = """
		Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
		sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
		Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut 
		aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in 
		voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint 
		occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
		""";

	private string MultiLineIndentedValue =
		 """
		Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
		sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
							Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut 
			  		aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in 
		voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint 
							occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit 
					  anim id est laborum.
		""";


	private string MultiLineMissDelimited = """
		Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
		sed do eiusmod tempor incididunt ut labore et dolore- magna aliqua. 
		Ut enim ad minim: veniam, quis nostrud exercitation ullamco laboris nisi ut 
		aliquip ex ea commodo consequat= Duis aute irure dolor in reprehenderit in 
		voluptate velit esse cillum dolore eu-fugiat nulla pariatur. Excepteur sint 
		occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
		""";

	private string MultiLineMissDelimitedIndented = """
		Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
			sed do eiusmod tempor incididunt ut labore et dolore- magna aliqua. 
			Ut enim ad minim: veniam, quis nostrud exercitation ullamco laboris nisi ut 
		      aliquip ex ea commodo consequat= Duis aute irure dolor in reprehenderit in 
				voluptate velit esse cillum dolore eu-fugiat nulla pariatur. Excepteur sint 
			    occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
		""";

	string BaseNestedStructure = TestHelpers.GetNestedStructure('\t', 1, '=');

	private string AppendStructure = "\nLevel0Array2 =\n \t = Value1/1\n\t= Value1/2";

	protected string AppendedStructure => BaseNestedStructure + AppendStructure;
	protected string InsertStructure => BaseNestedStructure +
		"\n\t\t\tLevel3Array3 = " +
		"\n\t\t\t\t=Value4/1" +
		"\n\t\t\t\t=Value4/2" +
		AppendStructure;


	protected string InsertMultilineValue => BaseNestedStructure + '\n' + TestHelpers.IndentLevel('\t', 5, "", 1) + "Level5Record3Value3 = " + MultiLineValue + '\n' + AppendStructure;
	protected string InsertMultilineIndentedValue => BaseNestedStructure + '\n' + TestHelpers.IndentLevel('\t', 5, "", 1) + "Level5Record3Value3 = " + MultiLineIndentedValue + '\n' + AppendStructure;


	List<(char Indent, int Step, char Delimiter)> tests =
		[
		(' ', 2, '='),
		(' ', 2, ':'),
		("\u2192"[0], 1, '-')
		];


	[Fact]
	public void VerifyDefaultParser()
	{

		var parser = Parser.FromFile("");
		Assert.Equal('\t', parser.Indent);
		Assert.Equal(1, parser.IndentStep);
		Assert.Equal('=', parser.Delimiter);
	}


	[Fact]
	public void VerifyParserMeta()
	{
		string ccl = meta(' ', 2, ':');

		var parser = Parser.FromContent("", ccl);

		Assert.Equal(' ', parser.Indent);
		Assert.Equal(2, parser.IndentStep);
		Assert.Equal(':', parser.Delimiter);
	}

	[Fact]
	public void RunBaseTests()
	{

		var parser = Parser.FromContent("", "");

		RunTest(BaseNestedStructure, nameof(BaseNestedStructure), parser);

		foreach (var test in tests)
		{
			string m = meta(test.Indent, test.Step, test.Delimiter);
			var baseStructure = m + '\n' + TestHelpers.GetNestedStructure(test.Indent, test.Step, test.Delimiter) + '\n';

			parser = Parser.FromContent("", baseStructure);
			Assert.Equal(test.Indent, parser.Indent);
			Assert.Equal(test.Indent, parser.Indent);
			Assert.Equal(test.Delimiter, parser.Delimiter);

			RunTest(baseStructure, nameof(BaseNestedStructure), parser);
		}


	}

	[Fact]
	public void RunAppendTests()
	{

		var parser = Parser.FromContent("", "");

		RunTest(AppendedStructure, nameof(AppendedStructure), parser);

		foreach (var test in tests)
		{
			string m = meta(test.Indent, test.Step, test.Delimiter);
			var baseStructure = m + '\n' + TestHelpers.GetNestedStructure(test.Indent, test.Step, test.Delimiter) + '\n';
			parser = Parser.FromContent("", baseStructure);
			var append = TestHelpers.InsertArrayValues(3, 3, 2, parser);

			RunTest(baseStructure + append, nameof(AppendedStructure), parser);
		}


	}
	[Fact]
	public void RunMultiLineTests()
	{

		var parser = Parser.FromContent("", "");

		RunTest(InsertMultilineValue, nameof(InsertMultilineValue), parser);

		foreach (var test in tests)
		{
			parser = Parser.FromContent("", meta(test.Indent, test.Step, test.Delimiter));

			var baseStructure = TestHelpers.GetNestedStructure(parser.Indent, parser.IndentStep, parser.Delimiter) + '\n';
			var insertMultiLine = baseStructure + TestHelpers.IndentLevel(TestHelpers.IndentLevel(parser.Indent, 3, $"Key{parser.Delimiter}", parser.IndentStep), 3, MultiLineValue);

			RunTest(insertMultiLine, nameof(InsertMultilineValue), parser);

		}


	}
	[Fact]
	public void RunIndentedMultiLineTests()
	{

		var parser = Parser.FromContent("", "");
		RunTest(InsertMultilineIndentedValue, nameof(InsertMultilineIndentedValue), parser);

		foreach (var test in tests)
		{
			parser = Parser.FromContent("", meta(test.Indent, test.Step, test.Delimiter));

			var baseStructure = TestHelpers.GetNestedStructure(parser.Indent, parser.IndentStep, parser.Delimiter) + '\n';
			var insertIndentedMultiLine = baseStructure + TestHelpers.IndentLevel(TestHelpers.IndentLevel(parser.Indent, 3, $"Key{parser.Delimiter}", parser.IndentStep), 3, MultiLineIndentedValue);

			RunTest(insertIndentedMultiLine, nameof(InsertMultilineIndentedValue), parser);


		}


	}

	[Fact]
	public void TestMissDelimited()
	{
		string control = TestHelpers.RemoveAllWhiteSpace(MultiLineMissDelimited, '\t');
		string test = TestHelpers.RemoveAllWhiteSpace(MultiLineMissDelimitedIndented, '\t');

		Assert.Equal(control, test);
		foreach (var scenaro in tests)
		{			
			string ccl =
		$"""
		MissDelimitedTest {scenaro.Delimiter}
		{TestHelpers.IndentLevel(scenaro.Indent,1, "",scenaro.Step )}	Key {scenaro.Delimiter} Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
		sed do eiusmod tempor incididunt ut labore et dolore- magna aliqua. 
			Ut enim ad minim: veniam, quis nostrud exercitation ullamco laboris nisi ut 
		      aliquip ex ea commodo consequat= Duis aute irure dolor in reprehenderit in 
				voluptate velit esse cillum dolore eu-fugiat nulla pariatur. Excepteur sint 
			    occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
		""";
			var parser = Parser.FromContent("", meta(scenaro.Indent, scenaro.Step, scenaro.Delimiter));
			var res = parser.ParseContent("", ccl) as IUnitRecord;

			Assert.NotNull(res);

			var unit = res["Key"] as IUnitValue;
			Assert.NotNull(unit);

			test = TestHelpers.RemoveAllWhiteSpace(unit.Value, parser.Indent);

			Assert.Equal(control, test);

		}

	}
	private void RunTest(string ccl, string testName, Parser parser)
	{
		var unit = parser.ParseContent(testName, ccl);
		VerifyUnit(unit, parser, -1);
	}

	private void VerifyUnit(IUnit unit, Parser parser, int level = 0, int index = 0)
	{

		switch (unit)
		{
			case IUnitRecord structure:
				VerifyRecord(structure, level, parser);
				break;
			case IUnitArray array:
				VerifyArray(array, level, parser);
				break;
			case IUnitValue value:
				VerifyValue(value.Value, level, index, parser);
				break;
		}
	}

	private void VerifyArray(IUnitArray array, int level, Parser parser)
	{
		int index = 0;
		level++;
		foreach (var element in array.Elements)
		{
			index++;
			VerifyUnit(element, parser, level, index);
		}
	}

	private void VerifyRecord(IUnitRecord structure, int level, Parser parser)
	{
		int index = 0;
		level++;
		foreach (var field in structure.FieldNames)
		{
			index++;
			var unit = structure[field];
			VerifyUnit(unit, parser, level, index);
		}
	}

	private void VerifyValue(string value, int level, int i, Parser parser)
	{
		if (value.StartsWith("Lorem"))
			VerifyMultiLine(value, parser);
		else
		{
			int newLine = value.IndexOf('\n');
			if (newLine == -1)
				newLine = value.Length;

			string expectedValue =$"Value{level}/{i}" ;
			string trimmedValue = value[..newLine].Trim();

			Assert.Equal(expectedValue, trimmedValue);
		}
	}

	private void VerifyMultiLine(string value, Parser parser)
	{
		string actual = MultiLineValue;
		string expected = TestHelpers.RemoveAllWhiteSpace(actual, parser.Indent);
		string test = TestHelpers.RemoveAllWhiteSpace(value, parser.Indent);
		Assert.Equal(expected, test);

	}

}
