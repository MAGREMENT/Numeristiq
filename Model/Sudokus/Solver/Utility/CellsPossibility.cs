using System;
using System.Collections.Generic;
using System.Linq;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Utility;

public class CellsPossibility : ISudokuElement
{
    public int Possibility { get; }
    public Cell[] Cells { get; }

    public CellsPossibility(int possibility, params Cell[] cells)
    {
        Possibility = possibility;
        Cells = cells;
    }

    public int DifficultyRank => 2;
    
    public CellPossibilities[] EveryCellPossibilities()
    {
        var result = new CellPossibilities[Cells.Length];
        for (int i = 0; i < Cells.Length; i++)
        {
            result[i] = new CellPossibilities(Cells[i], Possibility);
        }

        return result;
    }

    public Cell[] EveryCell()
    {
        return Cells;
    }

    public ReadOnlyBitSet16 EveryPossibilities()
    {
        return new ReadOnlyBitSet16(Possibility);
    }

    public CellPossibility[] EveryCellPossibility()
    {
        var result = new CellPossibility[Cells.Length];
        for (int i = 0; i < Cells.Length; i++)
        {
            result[i] = new CellPossibility(Cells[i], Possibility);
        }

        return result;
    }

    public IEnumerable<int> EnumeratePossibilities()
    {
        yield return Possibility;
    }

    public IEnumerable<CellPossibilities> EnumerateCellPossibilities()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            yield return new CellPossibilities(Cells[i], Possibility);
        }
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        return Cells;
    }

    public IEnumerable<CellPossibility> EnumerateCellPossibility()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            yield return new CellPossibility(Cells[i], Possibility);
        }
    }

    public bool Contains(Cell cell)
    {
        return Cells.Contains(cell);
    }

    public bool Contains(CellPossibility cp)
    {
        return Possibility == cp.Possibility && Cells.Contains(cp.ToCell());
    }

    public bool Contains(int possibility)
    {
        return Possibility == possibility;
    }

    public bool Contains(CellPossibilities cp)
    {
        return cp.Possibilities.Count == 1 && Contains(cp.Possibilities.FirstPossibility()) && Contains(cp.Cell);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CellsPossibility cp || cp.Possibility != Possibility 
            || cp.Cells.Length != Cells.Length) return false;
        foreach (var c in cp.Cells)
        {
            if (!Cells.Contains(c)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var c in Cells)
        {
            hash ^= c.GetHashCode();
        }

        return HashCode.Combine(Possibility, Cells.Length, hash);
    }

    public override string ToString()
    {
        return $"{Possibility}{Cells.ToStringSequence(", ")}";
    }
}