using System.Collections.Generic;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Positions;

public interface IReadOnlyGridPositions : IEnumerable<Cell>
{
    GridPositions Copy();

    GridPositions Difference(IReadOnlyGridPositions with);
}