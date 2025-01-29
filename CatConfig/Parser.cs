using System;
using System.Reflection.Emit;

namespace CatConfig;

public class Parser
{
	private const char DefaultIndent = '\t';
	private const int DefaultIndentStep = 1;
	private const char DefaultDelimiter = '=';
	private static Parser parser = new('\t', 1, '=');

	public char Indent { get; } = DefaultIndent;
	public int IndentStep { get; } = DefaultIndentStep;
	public char Delimiter { get; } = DefaultDelimiter;

	internal Parser(char indent, int step, char delimiter)
	{
		Indent = indent;
		IndentStep = step;
		Delimiter = delimiter;
	}

	public IUnit ParseFile(string filePath)
	{
		string content = File.ReadAllText(filePath);
		return ParseContent(filePath, content);
	}

	public IUnit ParseContent(string path, string content)
	{
		int start = GetEndOfMeta(content);

		return ParseContentInternal(path, content[start..], this);
	}


	public static Parser FromFile(string filePath)
	{
		string content = "";

		if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			content = File.ReadAllText(filePath);

		return FromContent(filePath, content);
	}


	public static Parser FromContent(string path, string content)
	{
		var end = GetEndOfMeta(content);
		if (end <= 0)
			return parser;

		return GetMetaParser(path, content[..end]);
	}
	private static int GetEndOfMeta(string content)
	{
		var firstKey = ParserHelpers.GetKey(content, 0,  DefaultDelimiter, DefaultIndent, DefaultIndentStep);
		int start = 0;

		if (firstKey.End > firstKey.Start)
		{
			string key = content[firstKey.Start..firstKey.End].Trim();
			if (key.Equals("meta", StringComparison.OrdinalIgnoreCase))
				start = ParserHelpers.GetDistanceToNextSibling(content, 0, 0, DefaultDelimiter, DefaultIndent, DefaultIndentStep);

		}

		return start;
	}



	private static Parser GetMetaParser(string path, string ccl)
	{
		var meta = ParseContentInternal(path, ccl, parser);
		return GetMetaParser(meta);
	}

	private static IUnit ParseContentInternal(string path, string content, Parser parser)
	{
		char delimiter = parser.Delimiter;
		char indent = parser.Indent;
		int indentStep = parser.IndentStep;

		var key = ParserHelpers.GetKey(content, 0, delimiter, indent, indentStep);

		if (key.Level > 0)
			content = BackDent(content[key.LevelStart..], parser.Indent, parser.IndentStep);

		int index = content.IndexOf(delimiter);
		index = int.Clamp(index, -1, 0);

		Ccl tree = new(index, 0, path);
		ParserHelpers.Parse(content, tree, delimiter, indent, indentStep);

		return Constructor.GetStructure(tree, parser);
	}


	private static Parser GetMetaParser(IUnit check)
	{
		if (check is IUnitRecord meta && meta.Name.Equals("meta", StringComparison.OrdinalIgnoreCase))
		{
			var indentField = meta["Indent"] as IUnitValue;
			var stepField = meta["IndentStep"] as IUnitValue;
			var delimiterField = meta["Delimiter"] as IUnitValue;

			string indent = indentField?.Value ?? "'" + DefaultIndent.ToString() + "'";
			string step = stepField?.Value ?? "'" + DefaultIndentStep.ToString() + "'";
			string delimiter = delimiterField?.Value ?? "'='";
			int indentStep = 1;

			var indentChar = LiteralHelpers.GetCharLiteral(indent, '\'', DefaultIndent);
			var delimiterChar = LiteralHelpers.GetCharLiteral(delimiter, '\'', DefaultDelimiter);

			if (delimiterChar == '\0')
				delimiterChar = '=';

			if (!int.TryParse(step, out indentStep))
				indentStep = 1;

			return new Parser(indentChar, indentStep, delimiterChar);

		}

		return parser;
	}

	private static string BackDent(string content, char indent, int step)
	{
		int lastLine = 0;
		int nextLine = ParserHelpers.FindChar(content, 0, '\n');
		int level = 0;
		string backDented = "";

		//Get BackDent Level from this line
		while (level < content.Length && content[level] == indent)
			level++;

		level = level / step;

		while (nextLine > 0 && nextLine <= content.Length)
		{
			backDented += content[(lastLine + level)..nextLine] + '\n';
			lastLine = nextLine + 1;
			nextLine = ParserHelpers.FindChar(content, lastLine + 1, '\n');
		}

		return backDented;
	}

}