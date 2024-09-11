using System;
using System.Collections.Generic;
using Model.Utility.BitSets;

namespace Model.Utility;

public readonly struct Cell
{
    public int Row { get; }
    public int Column { get; }


    public Cell(int row, int col)
    {
        Row = row;
        Column = col;
    }
    
    public bool IsAdjacentTo(Cell other)
    {
        var rowDiff = Math.Abs(Row - other.Row);
        var colDiff = Math.Abs(Column - other.Column);
        
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Cell coord) return false;
        return Row == coord.Row && Column == coord.Column;
    }

    public override string ToString()
    {
        return $"r{Row + 1}c{Column + 1}";
    }

    public static bool operator ==(Cell left, Cell right)
    {
        return left.Row == right.Row && left.Column == right.Column;
    }

    public static bool operator !=(Cell left, Cell right)
    {
        return !(left == right);
    }
}

public static class CellUtility
{ 
    public static bool AreAdjacent(IEnumerable<Cell> cells, Cell cell)
    {
        foreach (var c in cells)
        {
            if (c.IsAdjacentTo(cell)) return true;
        }

        return false;
    }
    
    public static IEnumerable<Cell[]> DivideInAdjacentCells(List<Cell> cells)
    {
        List<Cell> current = new();

        while (cells.Count > 0)
        {
            current.Add(cells[^1]);
            cells.RemoveAt(cells.Count - 1);
            var added = true;
            
            while (added)
            {
                added = false;
                for (int i = cells.Count - 1; i >= 0; i--)
                {
                    if (!AreAdjacent(current, cells[i])) continue;

                    current.Add(cells[i]);
                    cells.RemoveAt(i);
                    added = true;
                }
            }

            yield return current.ToArray();
            current.Clear();
        }
    }

    public static bool AreAllAdjacent(IReadOnlyList<Cell> cells)
    {
        InfiniteBitSet set = new();
        for (int i = 0; i < cells.Count - 1; i++)
        {
            for (int j = i + 1; j < cells.Count; j++)
            {
                if (!cells[i].IsAdjacentTo(cells[j])) continue;
                
                set.Add(i);
                set.Add(j);
            }
        }

        return set.Count == cells.Count;
    }
}