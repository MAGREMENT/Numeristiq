using Global;
using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtility.Graphs;

public interface ISudokuElement
{
    int DifficultyRank { get; }
    Possibilities EveryPossibilities();
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    CellPossibility[] EveryCellPossibility();
}