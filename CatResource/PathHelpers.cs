namespace CatResource;

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
    public static string[] GetAllNodes(string path)
    {
        List<string> parameters = new();
        string current = "";
        int index = 0;
        while (index < path.Length)
        {
            while (path[index] != '/')
            {
                current += path[index];
                index++;
            }

            if (!string.IsNullOrEmpty(current))
                parameters.Add(current);

            current = "";
            index++;
        }

        return parameters.ToArray();
    }
}