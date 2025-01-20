using CatConfig;

internal static class ParserHelpers
{

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
	public static int GetDistanceToNextSibling(string ccl, int start, int level, char delimiter, char indent, int indentStep)
	{
		int index = 0;
		int nextLine = 0;

		if (start == 0 || start < ccl.Length)
		{
			index = start + 1;
			bool first = true;
			int lvl = 0;

			//until: `lvl` == `level` (indicating next sibling)
			//or lvl < level (indicating no more siblings to find)
			while (index < ccl.Length && (first || lvl > level))
			{
				first = false;
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



	public static int GetNextKeyLineStart(string ccl, int index, int level, char delimiter, char indent, int indentStep)
	{
		int i = index;
		char c = ccl[i];

		if (i >= ccl.Length || (i > 0 && ccl[i] != '\n'))
			return i;

		if (i > 0 || c == '\n')
			i++;

		int nextLine = FindChar(ccl, i, '\n');
		if (nextLine == 0)
			nextLine = ccl.Length;

		int nextDelimiter = FindChar(ccl, i, delimiter);


		if (nextDelimiter > nextLine)
			return GetNextKeyLineStart(ccl, nextLine, level, delimiter, indent, indentStep);


		return i;
	}


	public static int FindChar(string ccl, int i, char c)
	{
		while (i < ccl.Length && ccl[i] != c)
			i++;

		return i;
	}

	public static (int start, int end) GetKey(string ccl, int index, int level, char delimiter, char indent, int indentStep)
	{
		int i = index;

		if (i > ccl.Length || i > 0 && ccl[i] != '\n')
			return (i, i);


		//the 'OR' case is important to avoid getting stuck on  new line
		if (i > 0 || (ccl.Length > 0 && ccl[i] == '\n'))
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
			{
				break;
			}


			if (c == '\n')
				return GetKey(ccl, i, level, delimiter, indent, indentStep);

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


	public static int GetNextLevel(string ccl, int start, int level, char delimiter, char indent, int indentStep)
	{
		if (ccl.Length > 0 && (start == 0 || start < ccl.Length && ccl[start] == '\n'))
		{
			int index = start;
			if (index > 0 || ccl[index] == '\n')
				index++;


			int nextLine = FindChar(ccl, index, '\n');
			if (nextLine == 0)
				nextLine = ccl.Length;

			int nextEquals = FindChar(ccl, index, delimiter);

			//line contains no Delimiter?
			if (nextLine < nextEquals)
				return GetNextLevel(ccl, nextLine, level, delimiter, indent, indentStep);

			int i = 0;

			while (index + i < ccl.Length && ccl[index + i] == indent)
				i++;

			level = i / indentStep;
		}

		return level;
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

	public static int Parse(string ccl, Ccl parent, char delimiter, char indent, int indentStep, int index = 0, int level = 0)
	{

		int last = -1;

		if (parent.Id < 0)
			return parent.Id;

		while (index < ccl.Length)
		{
			if (index == last)
			{//no changes, find end of line

				index = FindChar(ccl, index, '\n');

				if (index == last)
					index++; //this is the end of the line, so move on

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

				int lineStart = index;

				if (ccl[lineStart] == '\n' && lineStart + 1 < ccl.Length)
					lineStart++;

				index = keyEnd;
				string value = "";

				if (delay)
				{
					index = FindChar(ccl, index, '\n');
					int delayLength = GetDistanceToNextSibling(ccl, index, level, delimiter, indent, indentStep);
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
}