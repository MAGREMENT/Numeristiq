using System.Collections.Generic;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AlignedPairExclusionStrategy : IStrategy
{
    public string Name => "Aligned pair exclusion";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    private readonly int _maxAlzSize;

    public AlignedPairExclusionStrategy(int maxAlsSize)
    {
        _maxAlzSize = maxAlsSize;
    }

    public void ApplyOnce(ISolverView solverView)
    {
        for(int i = 0; i < 81; i++)
        {
            int row1 = i / 9;
            int col1 = i % 9;
            if(solverView.Sudoku[row1, col1] != 0) continue;

            for (int j = i + 1; j < 81; j++)
            {
                int row2 = j / 9;
                int col2 = j % 9;

                if (solverView.Sudoku[row2, col2] != 0) continue;
                if (Search(solverView, row1, col1, row2, col2)) return;
            }
        }
    }

    private bool Search(ISolverView solverView, int row1, int col1, int row2, int col2)
    {
        List<Coordinate> shared = new List<Coordinate>(
            Coordinate.SharedSeenEmptyCells(solverView, row1, col1, row2, col2));
        
        if (shared.Count < solverView.Possibilities[row1, col1].Count ||
            shared.Count < solverView.Possibilities[row2, col2].Count) return false;
        var inSameUnit = Coordinate.ShareAUnit(row1, col1, row2, col2);

        List<int[]> doubles = AllDoublesCombination(solverView.Possibilities[row1, col1],
                solverView.Possibilities[row2, col2], !inSameUnit);

        foreach (var als in AlmostLockedSet.SearchForSingleCellAls(solverView, shared))
        {
            RemoveDoubles(doubles, als);
        }

        for (int i = 2; i <= _maxAlzSize; i++)
        {
            foreach (var als in AlmostLockedSet.SearchForMultipleCellsAls(solverView, shared, i))
            {
                RemoveDoubles(doubles, als);
            }

            if (CheckForAbsentees(solverView, doubles, solverView.Possibilities[row1, col1],
                    solverView.Possibilities[row2, col2], row1, col1, row2, col2)) return true;
        }

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

    private void RemoveDoubles(List<int[]> doubles, AlmostLockedSet toRemove)
    {
        doubles.RemoveAll(ints => ints[0] != ints[1] && 
                                  toRemove.Possibilities.Peek(ints[0]) && toRemove.Possibilities.Peek(ints[1]));
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
}

