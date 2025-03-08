using CatConfig;

namespace CatConfig;

public record NoValue(int Level,int Id = 0) : IUnit;
public record EmptyValue(int Id,int Level) : IUnit, IEmptyUnit;
