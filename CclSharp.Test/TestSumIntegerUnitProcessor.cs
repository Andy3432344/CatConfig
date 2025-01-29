using CatConfig;

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

	public IUnit ResolveDelayedUnit(IDelayedUnit delayed)
	{
		if (!delayed.GetHostName().Equals(Name, StringComparison.OrdinalIgnoreCase))
			return noValue;

		if (!delayed.GetProtocolSchema().Equals(ProtocolSchema, StringComparison.OrdinalIgnoreCase))
			return noValue;

		string parseX = "";
		string parseY = "";

		int x = 0;
		int y = 0;

		string path = delayed.GetPath();
		int i = 0;
		int phase = 0;

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

		return new UnitValue(delayed.Id, (x + y).ToString());

	}
}
