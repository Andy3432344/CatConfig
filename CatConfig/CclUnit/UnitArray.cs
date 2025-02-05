using CatConfig;

namespace CatConfig;

public record UnitArray(int Id, IUnit[] Elements) : IUnitArray, IComplexUnit;
