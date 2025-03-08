
using System.Reflection.Emit;

namespace CatConfig.CclUnit;

public record NoRecord(int level=0) : IUnitRecord
{

    private readonly NoValue noUnit = new(level);
    public IUnit this[string fieldName] => noUnit;

    public IUnit this[(string field, string[] args) a] => noUnit;

    public Function this[IDelayedUnit field] => _ => noUnit;

    public string Name => nameof(NoRecord);
    public string[] FieldNames => [];
    public int Id => -1;
	public int Level { get; } = level;

	public IUnitRecord Transform(IUnitRecord import, bool @override = false) => this;
}
