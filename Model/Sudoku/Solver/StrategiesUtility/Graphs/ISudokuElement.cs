using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.StrategiesUtility.Graphs;

public interface ISudokuElement
{
    int DifficultyRank { get; }
    ReadOnlyBitSet16 EveryPossibilities();
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    CellPossibility[] EveryCellPossibility();
}