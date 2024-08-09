using Model.Core;
using Model.Repositories;

namespace Repository;

public abstract class HardCodedStrategyRepository<T> : IStrategyRepository<T> where T : Strategy
{
    public void SetStrategies(IReadOnlyList<T> list)
    {
        
    }

    public abstract IEnumerable<T> GetStrategies();

    public void UpdateStrategy(T strategy)
    {
        
    }

    public void AddPreset(IReadOnlyList<T> list, Stream stream)
    {
       
    }

    public IEnumerable<T> GetPreset(Stream stream)
    {
        return Enumerable.Empty<T>();
    }
}