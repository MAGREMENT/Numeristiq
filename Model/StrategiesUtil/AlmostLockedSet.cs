using System.Collections.Generic;
using Model.Possibilities;

namespace Model.StrategiesUtil;

public class AlmostLockedSet
{
    private readonly Coordinate[] _coords;
    public IPossibilities Possibilities { get; }

    public AlmostLockedSet(IPossibilities possibilities, params Coordinate[] coords)
    {
        Possibilities = possibilities.Copy();
        _coords = coords;
    }

    public List<Coordinate> SharedSeenCells(AlmostLockedSet set)
    {
        List<Coordinate> result = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = new Coordinate(row, col);
                if (SharedSeenCell(current, this, set)) result.Add(current);
            }
        }

        return result;
    }

    public bool IsSeenByAll(Coordinate coord)
    {
        foreach (var c in _coords)
        {
            if (c.Equals(coord) || !c.ShareAUnit(coord)) return false;
        }

        return true;
    }

    private static bool SharedSeenCell(Coordinate current, AlmostLockedSet one, AlmostLockedSet two)
    {
        foreach (var coord in one._coords)
        {
            if (!current.ShareAUnit(coord) || coord.Equals(current)) return false;
        }
        
        foreach (var coord in two._coords)
        {
            if (!current.ShareAUnit(coord) || coord.Equals(current)) return false;
        }

        return true;
    }
}