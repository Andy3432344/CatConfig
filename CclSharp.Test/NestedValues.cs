using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using CatConfig;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace CclSharp.Test;

public class NestedValues
{
	private Parser parser = Parser.FromFile("");

	const string MultiLineValue =
		"""
		Level5Record3Value3 = Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
		sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
		Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut 
		aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in 
		voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint 
		occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
		""";

	const string MultiLineIndentedValue =
		"""
							Level5Record3Value3 = Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
		   sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
							Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut 
			  		aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in 
		 voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint 
							occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit 
					  anim id est laborum.
		""";

	const string BaseNestedStructure = """
        Level0Record1 = 
        	Level1Record1Value1 = Value1/1
        	Level1Record1Array2 = 
        		=Value2/1
        		=Value2/2
        	Level1Record3 =
        		Level2Record1=
        			Level3Value1 = Value3/1

             
         						

        			Level3Record2 =
        				Level4Record1 = 
        					Level5Record1Value1 = Value5/1
        					Level5Record1Value2 = Value5/2
        				Level4Record2 = 
        					Level5Record2Value1 = Value5/1
        					Level5Record2Array2 = 
        						= Value6/1
        						= Value6/2
        """;

	const string AppendStructure = "\nLevel0Array2 =\n \t = Value1/1\n\t= Value1/2";

	protected string AppendedStructure => BaseNestedStructure + AppendStructure;
	protected string InsertStructure => BaseNestedStructure +
		"\n\t\t\tLevel3Array3 = " +
		"\n\t\t\t\t=Value4/1" +
		"\n\t\t\t\t=Value4/2" +
		AppendStructure;


	protected string InsertMultilineValue => BaseNestedStructure + '\n' + MultiLineValue + '\n' + AppendStructure;
	protected string InsertMultilineIndentedValue => BaseNestedStructure + '\n' + MultiLineIndentedValue + '\n' + AppendStructure;


	[Fact]
	public void VerifyNestedValues()
	{
		RunTest(BaseNestedStructure, nameof(BaseNestedStructure));
		RunTest(AppendedStructure, nameof(AppendedStructure));
		RunTest(InsertStructure, nameof(InsertStructure));
	}
	[Fact]
	public void VerifyMultiLineValues()
	{
		RunTest(InsertMultilineValue, nameof(InsertMultilineValue));
		RunTest(InsertMultilineIndentedValue, nameof(InsertMultilineIndentedValue));
	}

	private void RunTest(string ccl, string testName)
	{
		var tokens = parser.ParseContent(testName, ccl);
		var testValue = Constructor.GetStructure(tokens);

		VerifyUnit(testValue, -1);
	}

	private void VerifyUnit(IUnit unit, int level = 0, int index = 0)
	{

		switch (unit)
		{
			case IUnitRecord structure:
				VerifyRecord(structure, level);
				break;
			case IUnitArray array:
				VerifyArray(array, level);
				break;
			case IUnitValue value:
				VerifyValue(value.Value, level, index);
				break;
		}
	}

	private void VerifyArray(IUnitArray array, int level)
	{
		int index = 0;
		level++;
		foreach (var element in array.Elements)
		{
			index++;
			VerifyUnit(element, level, index);
		}
	}

	private void VerifyRecord(IUnitRecord structure, int level)
	{
		int index = 0;
		level++;
		foreach (var field in structure.FieldNames)
		{
			index++;
			var unit = structure[field];
			VerifyUnit(unit, level, index);
		}
	}

	private void VerifyValue(string value, int level, int i)
	{
		if (value.StartsWith("Lorem"))
			VerifyMultiLine(value);
		else
			Assert.Equal($"Value{level}/{i}", value);
	}

	private void VerifyMultiLine(string value)
	{
		string actual = MultiLineValue.Substring(MultiLineValue.IndexOf('=') + 1);
		string expected = RemoveAllWhiteSpace(actual,parser.Indent);
		string test = RemoveAllWhiteSpace(value,parser.Indent);
		Assert.Equal(expected, test);

	}

	private static string RemoveAllWhiteSpace(string value, char indent)
	{
		string text = "";
		int i = 0;

		while (i < value.Length)
		{
			char c = value[i];
			i++;
			if (char.IsWhiteSpace(c) || c == indent)
				continue;

			text += c;
		}
		return text;
	}
}
