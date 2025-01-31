namespace CatConfig;

public record NoValue(int Id = 0) : IUnit;
public record EmptyValue(int Id) : IUnit, IEmptyUnit;
