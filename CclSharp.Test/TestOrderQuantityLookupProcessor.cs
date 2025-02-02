using CatConfig;
using CatConfig.CclUnit;

namespace CclSharp.Test;

public class TestOrderQuantityLookupProcessor : IDelayedProcessor
{
	private NoValue noValue = new NoValue();

	private Dictionary<int, IUnitRecord> entities = new();

	public string Name { get; } = "OrderProcessor";
	public string ProtocolSchema { get; } = "test";

	public void DeliverRecord(int id, IUnitRecord hostRecord)
	{
		entities.TryAdd(id, hostRecord);
	}

	public IUnit ResolveDelayedUnit(int id, string name, UnitPath path)
	{
        string orderNumber = path[0];
		string column = path.Length > 1 ? path[1] : "";

		if (column.Equals("Quantity", StringComparison.OrdinalIgnoreCase))
			return new UnitValue(id, GetQuantity(orderNumber).ToString());

		return noValue;
	}


	private int GetQuantity(string parameter)
	{
		return parameter switch
		{
			"JM-323L" => 7,
			"HN/787K" => 12,
			"GB/121F" => 1,
			_ => 0
		};
	}
}