using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Graphs;

public interface ISudokuElement
{
    int DifficultyRank { get; }
    
    ReadOnlyBitSet16 EveryPossibilities();
    CellPossibilities[] EveryCellPossibilities();
    Cell[] EveryCell();
    CellPossibility[] EveryCellPossibility();

    IEnumerable<int> EnumeratePossibilities();
    IEnumerable<CellPossibilities> EnumerateCellPossibilities();
    IEnumerable<Cell> EnumerateCell();
    IEnumerable<CellPossibility> EnumerateCellPossibility();

    bool Contains(Cell cell);
    bool Contains(CellPossibility cp);
}