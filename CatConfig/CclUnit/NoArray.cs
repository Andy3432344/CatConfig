
namespace CatConfig;

public record NoArray(int level) : IUnitArray
{
    public IUnit[] Elements => [];
    public int Id => 0;
	public int Level { get; }= level;

}
