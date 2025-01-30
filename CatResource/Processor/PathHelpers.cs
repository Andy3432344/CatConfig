namespace CatResource.Processor
{
    internal static class PathHelpers
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
    }
}