namespace CatConfig;

public interface IUnitArray : IUnit
{
    IUnit[] Elements { get; }
}
