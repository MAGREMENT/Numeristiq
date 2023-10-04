using System.Collections.Generic;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Positions;

public interface IReadOnlyGridPositions : IEnumerable<Cell>
{
    GridPositions Copy();

    GridPositions Difference(IReadOnlyGridPositions with);

    static GridPositions DefaultDifference(IReadOnlyGridPositions from, IReadOnlyGridPositions with)
    {
        var result = from.Copy();
        foreach (var pos in with)
        {
            result.Remove(pos);
        }

        return result;
    }
}