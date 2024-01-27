using Global;
using Model.SudokuSolving.Solver.Possibility;

namespace Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

public interface ISudokuElement
{
    int DifficultyRank { get; }
    Possibilities EveryPossibilities();
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    CellPossibility[] EveryCellPossibility();
}