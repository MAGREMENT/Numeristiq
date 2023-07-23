using System.Collections.Generic;
using System.Linq;
using Model.Possibilities;

namespace Model.StrategiesUtil;

public class AlmostLockedSet
{
    private readonly Coordinate[] _coords;
    public IPossibilities Possibilities { get; }

    private AlmostLockedSet(Coordinate[] coords, IPossibilities poss)
    {
        _coords = coords;
        Possibilities = poss;
    }

    private AlmostLockedSet(Coordinate coord, IPossibilities poss)
    {
        _coords = new[] { coord };
        Possibilities = poss;
    }

    public bool Contains(Coordinate coord)
    {
        return _coords.Contains(coord);
    }


    public static IEnumerable<AlmostLockedSet> SearchForSingleCellAls(ISolverView solverView, List<Coordinate> coords)
    {
        foreach (var coord in coords)
        {
            IPossibilities current = solverView.Possibilities[coord.Row, coord.Col];
            if (current.Count == 2) yield return new AlmostLockedSet(coord, current);
        }
    }

    public static IEnumerable<AlmostLockedSet> SearchForMultipleCellsAls(ISolverView view, List<Coordinate> coords, int count)
    {
        List<AlmostLockedSet> result = new List<AlmostLockedSet>();
        for (int i = 0; i < coords.Count; i++)
        {
            SearchForMultipleCellsAls(view, result, view.Possibilities[coords[i].Row, coords[i].Col], coords,
                i, count - 1, count + 1, new List<Coordinate> {coords[i]});
        }

        return result;
    }
    
    private static void SearchForMultipleCellsAls(ISolverView view, List<AlmostLockedSet> result,
        IPossibilities current, List<Coordinate> coords, int start, int count, int type, List<Coordinate> visited)
    {
        for (int i = start + 1; i < coords.Count; i++)
        {
            if (!ShareAUnitWithAll(coords[i], visited) ||
                !ShareAtLeastOne(view.Possibilities[coords[i].Row, coords[i].Col], current)) continue;
            IPossibilities mashed = current.Mash(view.Possibilities[coords[i].Row, coords[i].Col]);
            if (count - 1 == 0)
            {
                if(mashed.Count == type) result.Add(new AlmostLockedSet(visited.ToArray(), mashed));
            }else if (mashed.Count <= type)
            {
                SearchForMultipleCellsAls(view, result, mashed, coords, i, count - 1, type,
                    new List<Coordinate>(visited) {coords[i]});
            }
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

    private static bool ShareAtLeastOne(IPossibilities one, IPossibilities two)
    {
        foreach (var possibility in one)
        {
            if (two.Peek(possibility)) return true;
        }

        return false;
    }
}