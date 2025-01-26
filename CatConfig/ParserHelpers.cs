﻿using System.Runtime.CompilerServices;
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


	public static int FindChar(string ccl, int i, char c)
	{
		while (i < ccl.Length && ccl[i] != c)
			i++;

		return i;
	}
	public record Key(int Start, int End, int Level, int LevelStart);

	/// <summary>
	/// Finds the next instance of ~delimiter~ that occurs on a line in which
	/// it is the first character, or is only preceded by indent characters
	/// </summary>
	/// <param name="ccl">Source text</param>
	/// <param name="index">Current level</param>
	/// <param name="delimiter">Parser delimiter</param>
	/// <param name="indent">Parser indent</param>
	/// <param name="indentStep">Parser indent step</param>
	/// <returns>An object which contains the Key start, end and line indexes, and the level of the key</returns>
	public static Key GetKey(string ccl, int index, char delimiter, char indent, int indentStep)
	{
		int i = index;
		int lastLine = i;

		//the 'OR' case is important to avoid getting stuck on a new line
		if (i > 0 || (ccl.Length > 0 && ccl[i] == '\n'))
			i++;

		int start = -1;
		bool whiteSpace = false;
		CclLevel levelCount = new(indentStep);

		while (i < ccl.Length)
		{
			char c = ccl[i];

			if (c == '\r')//windows line ending edge case
			{
				i++;
				continue;
			}

			if (c == '\n')
				lastLine++;
			else
			if (start < 0)
			{
				if (c != indent)
					levelCount.Reset();

				if (c == delimiter)
					break;
				else if (c == indent)
					levelCount.Step();
				else if (!char.IsWhiteSpace(c))
					start = i;
			}
			else
			{
				if (whiteSpace || c == delimiter) //if c is non whitespace, but not the delimiter
				{
					if (c == delimiter)
						break;//done

					if (!char.IsWhiteSpace(c))
					{
						//this must not be a key
						i = FindChar(ccl, i, '\n');
						start = -1;
						levelCount.HardReset();
						continue; //skip increment
					}
				}

				if (char.IsWhiteSpace(c) && !whiteSpace)//a key cannot contain whitespace
					whiteSpace = true;//but there can be spaces before the delimiter
			}

			i++;
		}

		if (start < 0)//no key found edge case
			start = i;

		if (i == ccl.Length)//end of string edge case
			lastLine = ccl.Length;

		return new(start, i, levelCount, lastLine);
	}


	private static (int start, int end) GetValue(string ccl, int index, int level, char delimiter, char indent, int indentStep)
	{
		if (ccl[index] != delimiter)
			return (index, index);
		else
			index++;

		int lastLineBreak = FindChar(ccl, index, '\n');
		if (lastLineBreak == 0)
			lastLineBreak = ccl.Length;

		var nextKey = GetKey(ccl, lastLineBreak, delimiter, indent, indentStep);
		return (index, nextKey.LevelStart);
	}




	public static int Parse(string ccl, Ccl parent, char delimiter, char indent, int indentStep, int index = 0, int level = 0)
	{
		int last = -1;

		if (parent.Id < 0)
			return parent.Id;

		var key = GetKey(ccl, index, delimiter, indent, indentStep);
		while (index < ccl.Length)
		{
			if (index == last)
			{
				if (ccl[index] == '\n')
					index++;
				else
					index = FindChar(ccl, index, '\n');

				continue;
			}

			last = index;

			if (key.Level != level)
				break;

			if (ccl.Length > key.End && ccl[key.End] == delimiter)
			{
				string keyName = ccl[key.Start..key.End].Trim();
				bool delay = IsDelayedValue(keyName);

				int lineStart = index;

				if (ccl[lineStart] == '\n' && lineStart + 1 < ccl.Length)
					lineStart++;

				index = key.End;
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
					(int valueStart, int valueEnd) = GetValue(ccl, index, level, delimiter, indent, indentStep);
					value = ccl[valueStart..valueEnd].Trim();
					index = FindChar(ccl, valueStart, '\n');
				}

				key = GetKey(ccl, index, delimiter, indent, indentStep);
				int nextLevel = key.Level;

				if (!string.IsNullOrEmpty(value) || key.Level <= level)
				{
					if (!parent.Items.TryGetValue(keyName, out var p))
						parent.Items[keyName] = p = new([new(key.Start, level, value)]);
					else
						p.Add(new(key.Start, level, value));
				}
				else
				{
					Ccl child = new(key.Start, level, keyName);

					if (key.Level > level)
						index = Parse(ccl, child, delimiter, indent, indentStep, index, key.Level);
					else
						index = Parse(ccl, parent, delimiter, indent, indentStep, index, nextLevel);

					if (!parent.Items.TryAdd(keyName, [child]))
						parent.Items[keyName].Add(child);

				}
			}
		}

		return index;
	}

	public static bool IsDelayedValue(string name)
	{
		return name.Length > 1 && name[0] == '{' && name[^1] == '}';
	}

}
