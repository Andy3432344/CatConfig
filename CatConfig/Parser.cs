using System;

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
		var ccl = parser.ParseContent(path, content);
		return GetMetaParser(ccl);
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

			var indentChar = GetCharLiteral(indent, '\'', '\t');
			var delimiterChar = GetCharLiteral(delimiter, '\'', '=');

			if (delimiterChar == '\0')
				delimiterChar = '=';

			if (!int.TryParse(step, out indentStep))
				indentStep = 1;

			return new Parser(indentChar, indentStep, delimiterChar);

		}

		return parser;
	}

	private static char GetCharLiteral(string value, char enclosed, char @default)
	{
		string ret = GetStringLiteral(value, enclosed);

		if (ret.Length == 1)
			return ret[0];

		return @default;


	}

	private static string GetStringLiteral(string value, char enclosed)
	{
		int i = 0;
		bool start = false;
		string ret = "";

		while (i < value.Length)
		{
			char c = value[i];

			if (c == enclosed)
			{
				if (!start)
					start = true;
				else
					break;
			}
			else if (start)
				ret += c;

			i++;
		}

		return ret;
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
		var nextKeyLength = NextKeyLength(content, 0, Delimiter, Indent);
		int index = nextKeyLength >= 0 ? 0 : -1;

		Ccl tree = new(index, 0, path);
		Parse(content, tree, Delimiter, Indent, IndentStep);

		return Constructor.GetStructure(tree);
	}

	private static int NextKeyLength(string ccl, int index, char delimiter, char indent)
	{
		int length = 0;

		if (ccl.Length > index && (index == 0 || ccl[index] == '\n'))
		{
			char c;

			while (index < ccl.Length && (c = ccl[index]) != delimiter)
			{
				if (length > 0 || c != indent)
				{
					length++;
					if (c == '\n')
						length = 0;
				}

				index++;
			}

		}

		if (index >= ccl.Length || ccl[index] != delimiter)
			return -1;

		if (ccl.Length < index)
			length = ccl.Length;

		return length;
	}

	private static int Parse(string ccl, Ccl parent, char delimiter, char indent, int indentStep, int index = 0, int level = 0)
	{

		int last = -1;

		if (parent.Id < 0)
			return parent.Id;

		while (index < ccl.Length)
		{
			if (index == last)
			{
				index = FindChar(ccl, index, '\n');

				if (index == last)
					index++;

				continue;
			}

			last = index;
			int currentLevel = GetNextLevel(ccl, index, level, delimiter, indent, indentStep);

			if (currentLevel != level)
				break;

			(int keyStart, int keyEnd) = GetKey(ccl, index, level, delimiter, indent, indentStep);
			int keyLength = keyEnd - keyStart;

			if (ccl[keyEnd] == delimiter)
			{
				string key = ccl[keyStart..keyEnd].Trim();
				bool delay = key.Length > 1 && key[0] == '{' && key[^1] == '}';
				int lineStart = index+1;
				index = keyEnd;
				string value = "";

				if (delay)
				{
					index = FindChar(ccl, index, '\n');
					int delayLength = DistanceToNextSibling(ccl, index, level, delimiter, indent, indentStep);
					int delayEnd = index + delayLength;
					value = ccl[lineStart..delayEnd];
					index += delayLength;
				}
				else
				{
					(int valueStart, int valueEnd) = GetValue(ccl, index, indent, delimiter);
					value = ccl[valueStart..valueEnd].Trim();
					index = FindChar(ccl, valueStart, '\n');
				}

				int nextLevel = GetLevelOfNextKey(ccl, index, level, delimiter, indent, indentStep);
				if (!string.IsNullOrEmpty(value) || nextLevel <= level)
				{
					if (!parent.Items.TryGetValue(key, out var p))
						parent.Items[key] = p = new([new(keyStart, level, value)]);
					else
						p.Add(new(keyStart, level, value));
				}
				else
				{
					Ccl child = new(keyStart, level, key);

					if (!parent.Items.TryAdd(key, new([child])))
						parent.Items[key].Add(child);

					Ccl p = parent;

					if (nextLevel > level)
					{
						p = child;
					}


					index = Parse(ccl, p, delimiter, indent, indentStep, index, nextLevel);
				}

			}

		}

		return index;
	}

	private static int GetLevelOfNextKey(string ccl, int index, int level, char delimiter, char indent, int indentStep)
	{
		int nextLevel = level;
		if (index < ccl.Length)
		{
			nextLevel = GetNextLevel(ccl, index, level, delimiter, indent, indentStep);

			if (nextLevel == 0)
			{
				while (index < ccl.Length && ccl[index] != '\n')
					index++;


				return GetLevelOfNextKey(ccl, index + 1, 0, delimiter, indent, indentStep);

			}
		}
		return nextLevel;
	}


	private static int GetNextLevel(string ccl, int start, int level, char delimiter, char indent, int indentStep)
	{
		if (start == 0 || (start < ccl.Length && ccl[start] == '\n'))
		{
			int index = start + 1;

			int nextLine = FindChar(ccl, index, '\n');
			if (nextLine == 0)
				nextLine = ccl.Length;

			int nextEquals = FindChar(ccl, index, delimiter);

			//line contains no Delimiter?
			if (nextLine < nextEquals)
				return GetNextLevel(ccl, nextLine, level, delimiter, indent, indentStep);

			int i = 0;
			// index++;
			while (index + i < ccl.Length && ccl[index + i] == indent)
				i++;

			level = i / indentStep;
		}

		return level;
	}

	/// <summary>
	/// Computes the number of characters in ccl that exist between the 
	/// 'start' index and the next New Line that precedes a line that is at
	/// a level equal to or less than 'level'.
	/// </summary>
	/// <param name="ccl">Test to search in</param>
	/// <param name="start">Index to start</param>
	/// <param name="level">Level of indentation to seek</param>
	/// <param name="delimiter">Character used to separate <Key> and <Value> ('=' by default)</param>
	/// <param name="indent">Character used to set line level('\t' by default)</param>
	/// <param name="indentStep">The number of 'indent' characters required to constitute one indent (1 by default)</param>
	/// <returns>Character-Distance from start index to next indent level</returns>
	private static int DistanceToNextSibling(string ccl, int start, int level, char delimiter, char indent, int indentStep)
	{
		int index = 0;
		int nextLine = 0;

		if (start == 0 || start < ccl.Length)
		{
			index = start + 1;
			int lvl = 0;

			//until: `lvl` == `level` (indicating next sibling)
			//or lvl < level (indicating no more siblings to find)
			while (index < ccl.Length && (lvl == 0 || lvl > level))
			{
				nextLine = FindChar(ccl, index, '\n');
				if (nextLine == 0)
					return ccl.Length - start;

				index = nextLine + 1;//beginning of line
				int i = 0;

				//until first character of key
				while (index + i < ccl.Length && ccl[index + i] == indent)
					i++;

				lvl = i / indentStep;
			}

		}

		return nextLine - start;
	}




	private static int FindChar(string ccl, int i, char c)
	{
		while (i < ccl.Length && ccl[i] != c)
			i++;

		return i;
	}

	private static (int start, int end) GetKey(string ccl, int index, int level, char delimiter, char indent, int indentStep)
	{
		int i = index;

		if (i > ccl.Length || i > 0 && ccl[i] != '\n')
			return (i, i);

		if (i > 0)
			i++;

		int lvlCount = 0;
		int start = -1;
		int end = 0;

		while (i < ccl.Length)
		{
			char c = ccl[i];

			if (c == indent)
				lvlCount++;

			if (lvlCount / indentStep > level)
				break;

			if (c != indent && start == -1)
			{
				start = i;
			}

			if (c == delimiter)
			{
				end = i;
				break;
			}

			i++;
		}

		if (start < 0)
			start = index;

		return (start, end);
	}

	private static (int start, int end) GetValue(string ccl, int index, char indent, char delimiter)
	{
		if (ccl[index] != delimiter)
			return (index, index);

		int startingIndex = index;
		index++;
		int valueStart = 0;

		int lastLineBreak = ccl.Length;

		while (index < ccl.Length && ccl[index] != delimiter)
		{
			char c = ccl[index];

			if (!char.IsWhiteSpace(c) && c != indent)
				if (valueStart == 0)
				{
					valueStart = index;
				}

			if (c == '\n')
			{
				lastLineBreak = index;
			}

			index++;
		}

		if (valueStart == 0)
			valueStart = index;

		if (index == ccl.Length)
			lastLineBreak = ccl.Length;

		if (index == ccl.Length || index < ccl.Length && ccl[index] == delimiter && lastLineBreak > valueStart)
		{
			return (valueStart, lastLineBreak);
		}

		return (startingIndex, startingIndex);
	}
}