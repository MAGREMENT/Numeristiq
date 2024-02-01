using Model.Sudoku.Solver.Possibility;
using Model.Utility;

namespace Model.Sudoku.Solver.StrategiesUtility.Graphs;

public interface ISudokuElement
{
    int DifficultyRank { get; }
    Possibilities EveryPossibilities();
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    CellPossibility[] EveryCellPossibility();
}