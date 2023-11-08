using System.Collections.Generic;
using Model.Solver.Positions;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.PossibilitiesPositions;

public interface IPossibilitiesPositions
{
    IEnumerable<int> EachPossibility();
    IEnumerable<Cell> EachCell();

    IEnumerable<Cell> EachCellWithPossibility(int possibility);
    IEnumerable<int> EachPossibilityWithCell(Cell cell);
    
    Possibilities Possibilities { get; }
    GridPositions Positions { get; }

    CellPossibilities[] ToCellPossibilitiesArray();
    
    int PossibilityCount { get; }
    int PositionsCount { get; }
}