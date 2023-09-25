using System;
using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil;

public class AlmostLockedSet
{
    public Cell[] Coordinates { get; }
    public IReadOnlyPossibilities Possibilities { get; }

    public AlmostLockedSet(Cell[] coordinates, IReadOnlyPossibilities poss)
    {
        if (coordinates.Length + 1 != poss.Count)
            throw new ArgumentException("Possibilities count not equal to cell count plus one"); 
        Coordinates = coordinates;
        Possibilities = poss;
    }

    public AlmostLockedSet(Cell coord, IReadOnlyPossibilities poss)
    {
        if(poss.Count != 2)
            throw new ArgumentException("Possibilities count not equal to cell count plus one"); 
        Coordinates = new[] { coord };
        Possibilities = poss;
    }

    public bool Contains(Cell coord)
    {
        foreach (var c in Coordinates)
        {
            if (c == coord) return true;
        }

        return false;
    }

    public bool HasAtLeastOneCoordinateInCommon(AlmostLockedSet als)
    {
        foreach (var coord in Coordinates)
        {
            foreach (var alsCoord in als.Coordinates)
            {
                if (coord == alsCoord) return true;
            }
        }

        return false;
    }

    public bool ShareAUnit(Cell coord)
    {
        foreach (var c in Coordinates)
        {
            if (!c.ShareAUnit(coord)) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostLockedSet als) return false;
        if (!Possibilities.Equals(als.Possibilities) || Coordinates.Length != als.Coordinates.Length) return false;
        foreach (var coord in Coordinates)
        {
            if (!als.Contains(coord)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int coordHashCode = 0;
        foreach (var coord in Coordinates)
        {
            coordHashCode ^= coord.GetHashCode();
        }

        return HashCode.Combine(Possibilities.GetHashCode(), coordHashCode);
    }

    public override string ToString()
    {
        var result = $"[ALS : {Coordinates[0].Row + 1}, {Coordinates[0].Col + 1} ";
        for (int i = 1; i < Coordinates.Length; i++)
        {
            result += $"| {Coordinates[i].Row + 1}, {Coordinates[i].Col + 1} ";
        }

        result += "=> ";
        foreach (var possibility in Possibilities)
        {
            result += $"{possibility}, ";
        }

        return result[..^2] + "]";
    }
}