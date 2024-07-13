using System.Collections.Generic;
using System.IO;
using Model.Core;

namespace Model.Repositories;

public interface IStrategyRepository<T> where T : Strategy
{
    void SetStrategies(IReadOnlyList<T> list);
    IReadOnlyList<T> GetStrategies();
    void UpdateStrategy(T strategy);
    void AddPreset(IReadOnlyList<T> list, Stream stream);
    IReadOnlyList<T> GetPreset(Stream stream);
}