using CatConfig;

namespace CclSharp.Test
{
	internal static class TestHelpers
	{
		public static string GetNestedStructure(Parser parser) =>
			GetNestedStructure(parser.Indent, parser.IndentStep, parser.Delimiter);



		public static string GetNestedStructure(char indentChar, int step, char delimiter) =>
		$"""
        Level0Record1 {delimiter} 
        {indent(indentChar, step)}Level1Record1Value1 {delimiter} Value1/1
        {indent(indentChar, step)}Level1Record1Array2 {delimiter} 
        {indent(indentChar, step)}{indent(indentChar, step)}{delimiter}Value2/1
        {indent(indentChar, step)}{indent(indentChar, step)}{delimiter}Value2/2
        {indent(indentChar, step)}Level1Record3 {delimiter}
        {indent(indentChar, step)}{indent(indentChar, step)}Level2Record1{delimiter}
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level3Value1 {delimiter} Value3/1

             
         {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}
         		{indent(indentChar, step)}
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level3Record2 {delimiter}
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level4Record1 {delimiter} 
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level5Record1Value1 {delimiter} Value5/1
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level5Record1Value2 {delimiter} Value5/2
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level4Record2 {delimiter} 
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level5Record2Value1 {delimiter} Value5/1
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}Level5Record2Array2 {delimiter} 
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{delimiter} Value6/1
        {indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{indent(indentChar, step)}{delimiter} Value6/2
        """;


		public static string InsertStructure(string insert, string before, string after, Parser p) => before + insert + after;
		//$"\n{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}Level3Array3 {p.Delimiter}" +
		//$"\n{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}{p.Delimiter}Value4/1" +
		//$"\n{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}{indent(p.Indent, p.IndentStep)}{p.Delimiter}Value4/2\n" +



		public static string InsertArrayValues(int insertLevel, int index, int valueCount, Parser p)
		{
			string levelIndent = string.Join("", Enumerable.Repeat(indent(p.Indent, p.IndentStep), insertLevel));


			string key = IndentLevel(p.Indent, insertLevel, "Level", p.IndentStep) + $"{insertLevel}Array{index}{p.Delimiter}\n";

			insertLevel++;

			return GetNestedValues(insertLevel, index, valueCount, p, key, new string[valueCount]);

		}

		private static string GetNestedValues(int level, int index, int valueCount, Parser p, string key, string[] fields)
		{
			string value = "";


			for (int i = 1; i <= valueCount; i++)
			{

				value += IndentLevel(p.Indent, level, "", p.IndentStep) + $"{fields[i - 1]}{p.Delimiter}Value{index + 1}/{i}\n";

			}
			return key + value;
		}

		public static string IndentLevel(string indent, int level, string text) => $"{indent}{text}";
		public static string IndentLevel(char indent, int level, string text, int step) => $"{TestHelpers.indent(indent, level * step)}{text}";


		private static string indent(char i, int s)
		{
			return new(Enumerable.Repeat(i, s).ToArray());
		}

		public static string RemoveAllWhiteSpace(string value, char indent)
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
}