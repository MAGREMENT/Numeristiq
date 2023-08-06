using System;
using System.Collections.Generic;
using System.Linq;
using Model.Possibilities;

namespace Model.StrategiesUtil;

public class AlmostLockedSet : ILinkGraphElement
{
    public Coordinate[] Coordinates { get; }
    public IPossibilities Possibilities { get; }

    public int Size => Coordinates.Length;

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
        return Coordinates.Contains(coord);
    }

    public IEnumerable<Coordinate> SharedSeenCells()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                Coordinate current = new Coordinate(row, col);

                if (ShareAUnitWithAll(current, Coordinates)) yield return current;
            }
        }
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

    public static List<AlmostLockedSet> SearchForAls(IStrategyManager view, List<Coordinate> coords, int max)
    {
        List<AlmostLockedSet> result = new();
        if (max < 1) return result;
        for(int i = 0; i < coords.Count; i++){
            IPossibilities current = view.Possibilities[coords[i].Row, coords[i].Col];
            if (current.Count == 2) result.Add(new AlmostLockedSet(coords[i], current));
            if (max > 1) SearchForAls(view, coords, new List<Coordinate> { coords[i] },
                current, i + 1, 2, max, result);
        }

        return result;
    }

    private static void SearchForAls(IStrategyManager view, List<Coordinate> coords, List<Coordinate> visited,
        IPossibilities current, int start, int count, int max, List<AlmostLockedSet> result)
    {
        for (int i = start; i < coords.Count; i++)
        {
            if (!ShareAUnitWithAll(coords[i], visited)) continue;

            IPossibilities mashed = current.Mash(view.Possibilities[coords[i].Row, coords[i].Col]);
            if (mashed.Count == current.Count + view.Possibilities[coords[i].Row, coords[i].Col].Count) continue;
            
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

            if (max > count) SearchForAls(view, coords, new List<Coordinate>(visited) { coords[i] },
                    mashed, i + 1, count + 1, max, result);
        }
    }

    public static IEnumerable<AlmostLockedSet> SearchForSingleCellAls(IStrategyManager strategyManager, List<Coordinate> coords)
    {
        foreach (var coord in coords)
        {
            IPossibilities current = strategyManager.Possibilities[coord.Row, coord.Col];
            if (current.Count == 2) yield return new AlmostLockedSet(coord, current);
        }
    }

    public static IEnumerable<AlmostLockedSet> SearchForMultipleCellsAls(IStrategyManager view, List<Coordinate> coords, int count)
    {
        List<AlmostLockedSet> result = new List<AlmostLockedSet>();
        for (int i = 0; i < coords.Count; i++)
        {
            SearchForMultipleCellsAls(view, result, view.Possibilities[coords[i].Row, coords[i].Col], coords,
                i, count - 1, count + 1, new List<Coordinate> {coords[i]});
        }

        return result;
    }
    
    private static void SearchForMultipleCellsAls(IStrategyManager view, List<AlmostLockedSet> result,
        IPossibilities current, List<Coordinate> coords, int start, int count, int type, List<Coordinate> visited)
    {
        for (int i = start + 1; i < coords.Count; i++)
        {
            if (!ShareAUnitWithAll(coords[i], visited) ||
                !ShareAtLeastOne(view.Possibilities[coords[i].Row, coords[i].Col], current)) continue;
            IPossibilities mashed = current.Mash(view.Possibilities[coords[i].Row, coords[i].Col]);
            var next = new List<Coordinate>(visited) {coords[i]};
            if (count - 1 == 0)
            {
                if(mashed.Count == type) result.Add(new AlmostLockedSet(next.ToArray(), mashed));
            }else if (mashed.Count <= type)
            {
                SearchForMultipleCellsAls(view, result, mashed, coords, i, count - 1, type,
                    next);
            }
        }
    }
    
    private static bool ShareAUnitWithAll(Coordinate current, IEnumerable<Coordinate> coordinates)
    {
        foreach (var coord in coordinates)
        {
            if (!current.ShareAUnit(coord)) return false;
        }

        return true;
    }

    private static bool ShareAtLeastOne(IPossibilities one, IPossibilities two)
    {
        foreach (var possibility in one)
        {
            if (two.Peek(possibility)) return true;
        }

        return false;
    }
}