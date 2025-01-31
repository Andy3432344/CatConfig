namespace CatConfig;

public record NoArray : IUnitArray
{
    public IUnit[] Elements => [];
    public int Id => 0;
}
