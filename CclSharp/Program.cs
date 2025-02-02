using CatConfig;
using CatConfig.CclParser;

namespace CclSharp;

internal class Program
{
    static void Main(string[] args)
    {
        string file = args[0];

        if (File.Exists(file))
        {
            var parser = Parser.FromFile("");

            var structure = parser.ParseFile(file);

            PrintStructure(structure, 0);
        }

    }





    public static void PrintStructure(IUnit structure, int indent)
    {

        switch (structure)
        {
            case IUnitValue v:
                Console.Write(v.Value);
                break;
            case IUnitArray a:
                indent += 2;
                Console.WriteLine();
                foreach (var e in a.Elements)
                {
                    Console.Write(GetIndent(indent) + '=');
                    PrintStructure(e, indent);
                    Console.WriteLine();
                }
                break;
            case IUnitRecord r:
                indent += 2;
                Console.WriteLine();
                foreach (var f in r.FieldNames)
                {
                    IUnit next = r[f];
                    Console.Write(GetIndent(indent) + f + " = ");
                    PrintStructure(next, indent);

                    if (next is IUnitValue)
                        Console.WriteLine();
                }
                break;
        }
    }


    private static string GetIndent(int indent)
    {
        return new string((Enumerable.Repeat(' ', indent).ToArray()));
    }


}
