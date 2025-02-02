using CatConfig;

namespace CatConfig
{
    internal static class InterpolationHelpers
	{
		public static (string, string, string) GetPathParts(string url)
		{
			string schema = "";
			string host = "";
			string path = "";

			int phase = 0;

			int i = 0;
			while (i < url.Length)
			{
				char c = url[i];

				switch (phase)
				{
					case 0://schema
						if (c == ':')
							phase = 1;
						else
							schema += c;
						break;
					case 1: // "//"
						if (c == '/')
							phase = 2;
						break;
					case 2: // "//"
						if (c == '/')
							phase = 3;
						break;
					case 3: //host
						if (c == '/')
							phase = 4;
						else
							host += c;
						break;
					case 4: //path
						path += c;
						break;
				}
				i++;
			}

			return (schema, host, path);
		}


		public static string? ResolveWithParameters(string name, string[] parameters, IUnitRecord rec, ref int index)
		{
			string? value = index < parameters.Length ? parameters[index] : null;
			string delayedName = '{' + name + '}';

			if (rec.FieldNames.Contains(delayedName, StringComparer.OrdinalIgnoreCase))
				name = delayedName;

			var field = rec[name];

			if (field is IDelayedUnit wait)
			{
				int a = wait.GetArity();
				if (a > parameters.Length)
					value = delayedName;
				else
				{
					field = rec[wait](parameters);
					index += a;
				}

				if (field is IUnitValue unitField)
					value = unitField.Value;
			}

			return value;
		}

		public static string ResolveWithRecord(IUnit val, IUnitRecord rec, string name)
		{
			string resolved = "";

			if (val is IUnitValue unit)
				resolved += unit.Value;
			else if (val is IDelayedUnit wait && wait.GetArity() == 0)
				return ResolveWithRecord(rec[wait](), rec, name);
			else
				return '{' + name + '}';

			return resolved;
		}
	}
}