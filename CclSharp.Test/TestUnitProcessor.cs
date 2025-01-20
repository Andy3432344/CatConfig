using CatConfig;

namespace CclSharp.Test
{
	public class TestUnitProcessor : IDelayedProcessor, IDelayedAccessor
	{
		private NoValue noValue = new NoValue();

		private Dictionary<int, IUnitRecord> entities = new();

		public string Name { get; } = "Test";
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

			delayed.GetHostRecord(this);

			if (!entities.TryGetValue(delayed.Id, out var urlRecord))
				return noValue;


			var xUnit = urlRecord["x"] as IUnitValue;
			var yUnit = urlRecord["y"] as IUnitValue;
			int x = 0;
			int y = 0;

			if (xUnit != null && yUnit != null)
			{
				int.TryParse(xUnit.Value, out x);
				int.TryParse(yUnit.Value, out y);
			}

			return new UnitValue(delayed.Id, (x + y).ToString());

		}
	}
}

