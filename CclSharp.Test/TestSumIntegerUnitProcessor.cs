using CatConfig;
using CatConfig.CclUnit;

namespace CclSharp.Test;

public class TestSumIntegerUnitProcessor : IDelayedProcessor
{
    private NoValue noValue = new NoValue();

    private Dictionary<int, IUnitRecord> entities = new();

    public string Name { get; } = "Sum";
    public string ProtocolSchema { get; } = "test";

    public void DeliverRecord(int id, IUnitRecord hostRecord)
    {
        entities.TryAdd(id, hostRecord);
    }

    public IUnit ResolveDelayedUnit(int id, string name, UnitPath fullPath)
    {
        string parseX = "";
        string parseY = "";

        int x = 0;
        int y = 0;


        int i = 0;
        int phase = 0;
        string path = fullPath[0];
        while (i < path.Length)
        {
            char c = path[i];

            if (phase == 0)
            {
                if (c == '+')
                    phase = 1;
                else
                    parseX += c;
            }
            else
            {
                parseY += c;
            }
            i++;
        }

        int.TryParse(parseX, out x);
        int.TryParse(parseY, out y);

        return new UnitValue(id, (x + y).ToString());

    }//
}
