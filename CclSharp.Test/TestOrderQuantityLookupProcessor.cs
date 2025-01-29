﻿using CatConfig;

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

	public IUnit ResolveDelayedUnit(IDelayedUnit delayed)
	{
		if (!delayed.GetHostName().Equals(Name, StringComparison.OrdinalIgnoreCase))
			return noValue;

		if (!delayed.GetProtocolSchema().Equals(ProtocolSchema, StringComparison.OrdinalIgnoreCase))
			return noValue;

		string current = "";
		List<string> parts = new();

		string path = delayed.GetPath();
		int i = 0;


		while (i < path.Length)
		{
			char c = path[i];


			if (c == '/')
			{
				if (!string.IsNullOrEmpty(current))
					parts.Add(current);
				current = "";
			}
			else
				current += c;

			i++;
		}

		if (!string.IsNullOrEmpty(current))
			parts.Add(current);


		string orderNumber = parts.FirstOrDefault() ?? "";
		string column = parts.Count > 1 ? parts[1] : "";
		if (column.Equals("Quantity", StringComparison.OrdinalIgnoreCase))
			return new UnitValue(delayed.Id, GetQuantity(orderNumber).ToString());

		return noValue;
	}


	private int GetQuantity(string parameter)
	{
		return parameter switch
		{
			"JM-323L" => 7,
			"HN-787K" => 12,
			"GB-121F" => 1,
			_ => 0
		};
	}
}