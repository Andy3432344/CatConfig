namespace CatConfig;

public record NoRecord : IUnitRecord
{

	private readonly NoValue noUnit = new();
	public IUnit this[string fieldName] => noUnit;

	public IUnit this[(string field, string[] args) a] => noUnit;

	public Function this[IDelayedUnit field] => _ => noUnit;

	public string Name => nameof(NoRecord);
	public string[] FieldNames => [];
	public int Id => -1;
}
