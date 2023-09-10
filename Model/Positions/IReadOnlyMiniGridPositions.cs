using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model.Positions;

public interface IReadOnlyMiniGridPositions : IEnumerable<Coordinate>
{
    public int Count { get; }
}