using System;
using System.Reflection.Emit;

namespace CatConfig;

public class Parser
{
	private static Parser parser = new('\t', 1, '=');

	public char Indent { get; } = '\t';
	public int IndentStep { get; } = 1;
	public char Delimiter { get; } = '=';

	internal Parser(char indent, int step, char delimiter)
	{
		Indent = indent;
		IndentStep = step;
		Delimiter = delimiter;
	}

	public static Parser FromContent(string path, string content)
	{
		var unit = parser.ParseContent(path, content);
		return GetMetaParser(unit);
	}


	private static Parser GetMetaParser(IUnit check)
	{
		if (check is IUnitRecord meta && meta.Name.Equals("meta", StringComparison.OrdinalIgnoreCase))
		{
			var indentField = meta["Indent"] as IUnitValue;
			var stepField = meta["IndentStep"] as IUnitValue;
			var delimiterField = meta["Delimiter"] as IUnitValue;

			string indent = indentField?.Value ?? "'\t'";
			string step = stepField?.Value ?? "1";
			string delimiter = delimiterField?.Value ?? "'='";
			int indentStep = 1;

			var indentChar = LiteralHelpers.GetCharLiteral(indent, '\'', '\t');
			var delimiterChar = LiteralHelpers.GetCharLiteral(delimiter, '\'', '=');

			if (delimiterChar == '\0')
				delimiterChar = '=';

			if (!int.TryParse(step, out indentStep))
				indentStep = 1;

			return new Parser(indentChar, indentStep, delimiterChar);

		}

		return parser;
	}

	public static Parser FromFile(string filePath)
	{
		string content = "";

		if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			content = File.ReadAllText(filePath);

		return FromContent(filePath, content);
	}


	public IUnit ParseFile(string filePath)
	{
		string content = File.ReadAllText(filePath);
		return ParseContent(filePath, content);
	}

	public IUnit ParseContent(string path, string content)
	{
		int currentLevel = ParserHelpers.GetNextLevel(content, 0, 0, Delimiter, Indent, IndentStep);

		if (currentLevel > 0)
		{
			var nextKey = ParserHelpers.GetNextKeyLineStart(content, 0, currentLevel, Delimiter, Indent, IndentStep);
			content = BackDent(content[nextKey..], Indent, IndentStep);
		}

		//if (content.Length > 1 && content[0] == Indent)

		int index = content.IndexOf(Delimiter);
		index = int.Clamp(index, -1, 0);

		Ccl tree = new(index, 0, path);
		ParserHelpers.Parse(content, tree, Delimiter, Indent, IndentStep);

		return Constructor.GetStructure(tree);
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