using System;
using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil.AlmostLockedSets;

public class AlmostLockedSet
{
    public Cell[] Cells { get; }
    public IReadOnlyPossibilities Possibilities { get; }

    public AlmostLockedSet(Cell[] cells, IReadOnlyPossibilities poss)
    {
        if (cells.Length + 1 != poss.Count)
            throw new ArgumentException("Possibilities count not equal to cell count plus one"); 
        Cells = cells;
        Possibilities = poss;
    }

    public AlmostLockedSet(Cell coord, IReadOnlyPossibilities poss)
    {
        if(poss.Count != 2)
            throw new ArgumentException("Possibilities count not equal to cell count plus one"); 
        Cells = new[] { coord };
        Possibilities = poss;
    }

    public bool Contains(Cell coord)
    {
        foreach (var c in Cells)
        {
            if (c == coord) return true;
        }

        return false;
    }

    public bool HasAtLeastOneCoordinateInCommon(AlmostLockedSet als)
    {
        foreach (var coord in Cells)
        {
            foreach (var alsCoord in als.Cells)
            {
                if (coord == alsCoord) return true;
            }
        }

        return false;
    }

    public bool ShareAUnit(Cell coord)
    {
        foreach (var c in Cells)
        {
            if (!c.ShareAUnit(coord)) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostLockedSet als) return false;
        if (!Possibilities.Equals(als.Possibilities) || Cells.Length != als.Cells.Length) return false;
        foreach (var coord in Cells)
        {
            if (!als.Contains(coord)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int coordHashCode = 0;
        foreach (var coord in Cells)
        {
            coordHashCode ^= coord.GetHashCode();
        }

        return HashCode.Combine(Possibilities.GetHashCode(), coordHashCode);
    }

    public override string ToString()
    {
        var result = $"[ALS : {Cells[0].Row + 1}, {Cells[0].Col + 1} ";
        for (int i = 1; i < Cells.Length; i++)
        {
            result += $"| {Cells[i].Row + 1}, {Cells[i].Col + 1} ";
        }

        result += "=> ";
        foreach (var possibility in Possibilities)
        {
            result += $"{possibility}, ";
        }

        return result[..^2] + "]";
    }
}