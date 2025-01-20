namespace CatConfig
{
	internal static class LiteralHelpers
	{

		public static char GetCharLiteral(string value, char enclosed, char @default)
		{
			string ret = GetStringLiteral(value, enclosed);

			if (ret.Length == 1)
				return ret[0];

			return @default;


		}

		public static string GetStringLiteral(string value, char enclosed)
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
	}
}