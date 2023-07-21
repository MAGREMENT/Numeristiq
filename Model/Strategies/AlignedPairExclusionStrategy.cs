using System.Collections.Generic;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AlignedPairExclusionStrategy : IStrategy
{
    public string Name => "Aligned pair exclusion";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(solverView.Sudoku[row, col] != 0) continue;

                for (int row2 = row; row2 < 9; row2++)
                {
                    for (int col2 = col + 1; col2 < 9; col2++)
                    {
                        if (solverView.Sudoku[row2, col2] != 0) continue;
                        if (Search(solverView, row, col, row2, col2)) return;
                    }
                }
            }
        }
    }

    private bool Search(ISolverView solverView, int row1, int col1, int row2, int col2)
    {
        List<Coordinate> shared = SharedSeenCells(solverView, row1, col1, row2, col2);
        if (shared.Count < solverView.Possibilities[row1, col1].Count ||
            shared.Count < solverView.Possibilities[row2, col2].Count) return false;
        var inSameUnit = Coordinate.ShareAUnit(row1, col1, row2, col2);

        List<int[]> doubles = AllDoublesCombination(solverView.Possibilities[row1, col1],
                solverView.Possibilities[row2, col2], !inSameUnit);

        foreach (var als in SearchForSingleCellAls(solverView, shared))
        {
            RemoveDoubles(doubles, als);
        }

        foreach (var als in SearchForMultipleCellsAls(solverView, shared, 2))
        {
            RemoveDoubles(doubles, als);
        }

        if (CheckForAbsentees(solverView, doubles, solverView.Possibilities[row1, col1],
                solverView.Possibilities[row2, col2], row1, col1, row2, col2)) return true;

        foreach (var als in SearchForMultipleCellsAls(solverView, shared, 3))
        {
            RemoveDoubles(doubles, als);
        }
        
        if (CheckForAbsentees(solverView, doubles, solverView.Possibilities[row1, col1],
                solverView.Possibilities[row2, col2], row1, col1, row2, col2)) return true;

        return false;
    }

    private List<int[]> AllDoublesCombination(IPossibilities one, IPossibilities two, bool countRepeats)
    {
        List<int[]> result = new();
        foreach (var poss1 in one)
        {
            foreach (var poss2 in two)
            {
                if(!countRepeats && poss1 == poss2) continue;
                result.Add(new []{poss1, poss2});
            }
        }

        return result;
    }

    private void RemoveDoubles(List<int[]> doubles, HashSet<int> toRemove)
    {
        doubles.RemoveAll(ints => ints[0] != ints[1] && 
                                  toRemove.Contains(ints[0]) && toRemove.Contains(ints[1]));
    }

    private bool CheckForAbsentees(ISolverView solverView, List<int[]> doubles, IPossibilities one, IPossibilities two,
        int row1, int col1, int row2, int col2)
    {
        bool wasProgressMade = false;
        HashSet<int> presenceOne = new();
        HashSet<int> presenceTwo = new();

        foreach (var iDouble in doubles)
        {
            presenceOne.Add(iDouble[0]);
            presenceTwo.Add(iDouble[1]);
        }

        if (presenceOne.Count != one.Count)
        {
            foreach (var poss in one)
            {
                if (!presenceOne.Contains(poss) && solverView.RemovePossibility(poss, row1, col1, this))
                    wasProgressMade = true;
            }
        }
        
        if (presenceTwo.Count != two.Count)
        {
            foreach (var poss in two)
            {
                if (!presenceTwo.Contains(poss) && solverView.RemovePossibility(poss, row2, col2, this))
                    wasProgressMade = true;
            }
        }
        
        return wasProgressMade;
    }

    private IEnumerable<HashSet<int>> SearchForSingleCellAls(ISolverView solverView, List<Coordinate> coords)
    {
        foreach (var coord in coords)
        {
            IPossibilities current = solverView.Possibilities[coord.Row, coord.Col];
            if (current.Count == 2) yield return new HashSet<int>(current);
        }
    }

    private IEnumerable<HashSet<int>> SearchForMultipleCellsAls(ISolverView view, List<Coordinate> coords, int count)
    {
        List<HashSet<int>> result = new List<HashSet<int>>();
        for (int i = 0; i < coords.Count; i++)
        {
            SearchForMultipleCellsAls(view, result, view.Possibilities[coords[i].Row, coords[i].Col], coords,
                i, count - 1, count + 1, new List<Coordinate> {coords[i]});
        }

        return result;
    }
    
    private void SearchForMultipleCellsAls(ISolverView view, List<HashSet<int>> result,
        IPossibilities current, List<Coordinate> coords, int start, int count, int type, List<Coordinate> visited)
    {
        for (int i = start + 1; i < coords.Count; i++)
        {
            if (!ShareAUnitWithAll(coords[i], visited) ||
                !ShareAtLeastOne(view.Possibilities[coords[i].Row, coords[i].Col], current)) continue;
            IPossibilities mashed = current.Mash(view.Possibilities[coords[i].Row, coords[i].Col]);
            if (count - 1 == 0)
            {
                if(mashed.Count == type) result.Add(new HashSet<int>(mashed));
            }else if (mashed.Count <= type)
            {
                SearchForMultipleCellsAls(view, result, mashed, coords, i, count - 1, type,
                    new List<Coordinate>(visited) {coords[i]});
            }
        }
    }

    private bool ShareAUnitWithAll(Coordinate current, List<Coordinate> coordinates)
    {
        foreach (var coord in coordinates)
        {
            if (!current.ShareAUnit(coord)) return false;
        }

        return true;
    }

    private bool ShareAtLeastOne(IPossibilities one, IPossibilities two)
    {
        foreach (var possibility in one)
        {
            if (two.Peek(possibility)) return true;
        }

        return false;
    }

    private List<Coordinate> SharedSeenCells(ISolverView solverView, int row1, int col1, int row2, int col2)
    {
        List<Coordinate> result = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Sudoku[row, col] != 0 ||
                    (row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                
                if (Coordinate.ShareAUnit(row, col, row1, col1)
                    && Coordinate.ShareAUnit(row, col, row2, col2))
                {
                    result.Add(new Coordinate(row, col));  
                }
            }
        }

        return result;
    }
}

