using System;
using System.Collections.Generic;
using System.Linq;
using Model.Possibilities;
using Model.StrategiesUtil.LoopFinder;

namespace Model.StrategiesUtil;

public class AlmostLockedSet : ILinkGraphElement
{
    public Coordinate[] Coordinates { get; }
    public IPossibilities Possibilities { get; }

    public AlmostLockedSet(Coordinate[] coordinates, IPossibilities poss)
    {
        Coordinates = coordinates;
        Possibilities = poss;
    }

    public AlmostLockedSet(Coordinate coord, IPossibilities poss)
    {
        Coordinates = new[] { coord };
        Possibilities = poss;
    }

    public bool Contains(Coordinate coord)
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

    public bool ShareAUnit(Coordinate coord)
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

    public PossibilityCoordinate[] EachElement()
    {
        //TODO
        return Array.Empty<PossibilityCoordinate>();
    }

    public static List<AlmostLockedSet> SearchForAls(IStrategyManager view, List<Coordinate> coords, int max)
    {
        List<AlmostLockedSet> result = new();
        if (max < 1) return result;
        for(int i = 0; i < coords.Count; i++){
            IPossibilities current = view.Possibilities[coords[i].Row, coords[i].Col];
            if (current.Count == 2) result.Add(new AlmostLockedSet(coords[i], current));
            if (max > 1) SearchForAls(view, coords, new List<Coordinate> { coords[i] },
                current, i + 1, max, result);
        }

        return result;
    }

    private static void SearchForAls(IStrategyManager view, List<Coordinate> coords, List<Coordinate> visited,
        IPossibilities current, int start, int max, List<AlmostLockedSet> result)
    {
        for (int i = start; i < coords.Count; i++)
        {
            if (!ShareAUnitWithAll(coords[i], visited)) continue;

            IPossibilities mashed = current.Mash(view.Possibilities[coords[i].Row, coords[i].Col]);
            if (mashed.Count == current.Count + view.Possibilities[coords[i].Row, coords[i].Col].Count) continue;
            int count = visited.Count + 1;
            
            if (mashed.Count == count + 1)
            {
                Coordinate[] final = new Coordinate[visited.Count + 1];
                for (int j = 0; j < visited.Count; j++)
                {
                    final[j] = visited[j];
                }

                final[^1] = coords[i];
                result.Add(new AlmostLockedSet(final, mashed));
            }

            if (max >= count) SearchForAls(view, coords, new List<Coordinate>(visited) { coords[i] },
                    mashed, i + 1, max, result);
        }
    }

    private static bool ShareAUnitWithAll(Coordinate current, List<Coordinate> coordinates)
    {
        foreach (var coord in coordinates)
        {
            if (!current.ShareAUnit(coord)) return false;
        }

        return true;
    }
}