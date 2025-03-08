using CatConfig;

namespace CatConfig;

public record UnitArray(int Id,int Level, IUnit[] Elements) : IUnitArray, IComplexUnit;
