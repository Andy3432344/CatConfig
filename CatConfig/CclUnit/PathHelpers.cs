namespace CatConfig.CclUnit;

public static class PathHelpers
{

    public static string GetPathNode(string path, ref int index)
    {
        if (path.Length == 0 || index >= path.Length || index > 0 && path[index] != '/')
            return "";

        char c;
        string format = "";
        while (index < path.Length && (c = path[index]) != '/')
        {
            format += c;
            index++;
        }

        return format;
    }
    public static string[] GetAllNodes(string path, char quoteChar)
    {
        List<string> parameters = new();
        string current = "";
        int index = 0;
        bool quote = false;

        while (index < path.Length)
        {

            while (path[index] != '/' || quote)
            {
                quote = quote ? path[index] != quoteChar :
                    path[index] == quoteChar;

                if (path[index] != quoteChar)
                    current += path[index];

                index++;

                if (index == path.Length)
                    break;
            }

            if (!string.IsNullOrEmpty(current))
                parameters.Add(current);

            current = "";
            index++;
        }

        return parameters.ToArray();
    }
}