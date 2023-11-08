using System.Collections.Generic;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.PossibilityPosition;

public interface IPossibilitiesPositions
{
    IEnumerable<Cell> EachCell();
    IEnumerable<Cell> EachCell(int with);
    
    IReadOnlyPossibilities PossibilitiesInCell(Cell cell);
    
    Possibilities Possibilities { get; }
    GridPositions Positions { get; }

    CellPossibilities[] ToCellPossibilitiesArray();
    
    int PossibilityCount { get; }
    int PositionsCount { get; }
}