namespace CatConfig.CclParser
{
    internal static class LiteralHelpers
    {

        public static char GetCharLiteral(string value, char enclosed, char @default)
        {
            string ret = value.GetStringLiteral(enclosed);

            if (ret.Length == 1)
                return ret[0];

            return @default;


        }

        public static string GetStringLiteral(this string value, char enclosed)
        {
            int i = 0;
            bool start = false;
            string ret = "";

            if (enclosed == '\0')
                return value;

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

            if (string.IsNullOrEmpty(ret))
                ret = value;

            return ret;
        }
    }
}