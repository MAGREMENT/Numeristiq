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

    public IEnumerable<Coordinate> SharedSeenCells(int row1, int col1, int row2, int col2)
    {
        bool isRow = true;
        bool isCol = true;
        bool isMini = true;

        foreach (var coord in _coords)
        {
            if (!(coord.Row == row1 && coord.Row == row2)) isRow = false;
            if (!(coord.Col == col1 && coord.Col == col2)) isCol = false;
            if (!(coord.Row / 3 == row1 / 3 && coord.Col / 3 == col1 / 3 &&
                  coord.Row / 3 == row2 / 3 && coord.Col / 3 == col2 / 3)) isMini = false;

            if (isRow && !isCol && !isMini)
            {
                for (int col = 0; col < 9; col++)
                {
                    if(col == col2 || col == col1 || _coords.Any(coordinate => coordinate.Col == col)) continue;
                    yield return new Coordinate(row1, col);
                }
                yield break;
            }

            if (!isRow && isCol && !isMini)
            {
                for (int row = 0; row < 9; row++)
                {
                    if (row == row1 || row == row2 || _coords.Any(coordinate => coordinate.Row == row)) continue;
                    yield return new Coordinate(row, col1);
                }
                yield break;
            }

            if (!isRow && !isCol && isMini)
            {
                int miniRow = row1 / 3;
                int miniCol = row2 / 3;
                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = miniRow + gridRow;
                        int col = miniCol + gridCol;
                        if ((row == row1 && col == col1) || (row == row2 && col == col2) ||
                            _coords.Any(coordinate => row == coordinate.Row && col == coordinate.Col)) continue;
                        yield return new Coordinate(row, col);
                    }
                }
                yield break;
            }
        }
    }
}