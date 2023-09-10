using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Positions;

public interface IReadOnlyMiniGridPositions : IEnumerable<Cell>
{
    public int Count { get; }
}