using System.Collections.Generic;
using Model.Core;

namespace Model.Repositories;

public interface IStrategyPresetRepository<T> where T : Strategy
{
    IEnumerable<T> GetStrategies();
    void SetStrategies(IReadOnlyList<T> strategies);
}

public interface IStrategyRepository<T> : IStrategyPresetRepository<T> where T : Strategy
{
    void UpdateStrategy(T strategy);
}