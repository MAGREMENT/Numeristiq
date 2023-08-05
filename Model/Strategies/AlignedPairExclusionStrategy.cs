using System.Collections.Generic;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class AlignedPairExclusionStrategy : IStrategy //TODO optimize
{
    public string Name => "Aligned pair exclusion";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    private readonly int _maxAlzSize;

    public AlignedPairExclusionStrategy(int maxAlsSize)
    {
        _maxAlzSize = maxAlsSize;
    }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for(int i = 0; i < 81; i++)
        {
            int row1 = i / 9;
            int col1 = i % 9;
            if(strategyManager.Sudoku[row1, col1] != 0) continue;

            for (int j = i + 1; j < 81; j++)
            {
                int row2 = j / 9;
                int col2 = j % 9;

                if (strategyManager.Sudoku[row2, col2] != 0) continue;
                if (Search(strategyManager, row1, col1, row2, col2)) return;
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        List<Coordinate> shared = new List<Coordinate>(
            Coordinate.SharedSeenEmptyCells(strategyManager, row1, col1, row2, col2));
        
        if (shared.Count < strategyManager.Possibilities[row1, col1].Count ||
            shared.Count < strategyManager.Possibilities[row2, col2].Count) return false;
        
        var inSameUnit = Coordinate.ShareAUnit(row1, col1, row2, col2);
        
        List<int[]> doubles = AllDoublesCombination(strategyManager.Possibilities[row1, col1],
            strategyManager.Possibilities[row2, col2], !inSameUnit);

        foreach (var als in AlmostLockedSet.SearchForAls(strategyManager, shared, _maxAlzSize))
        {
            RemoveDoubles(doubles, als);
            if(CheckForAbsentees(strategyManager, doubles, strategyManager.Possibilities[row1, col1],
                   strategyManager.Possibilities[row2, col2], row1, col1, row2, col2)) return true;
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

    private bool CheckForAbsentees(IStrategyManager strategyManager, List<int[]> doubles, IPossibilities one, IPossibilities two,
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
                if (!presenceOne.Contains(poss) && strategyManager.RemovePossibility(poss, row1, col1, this))
                    wasProgressMade = true;
            }
        }
        
        if (presenceTwo.Count != two.Count)
        {
            foreach (var poss in two)
            {
                if (!presenceTwo.Contains(poss) && strategyManager.RemovePossibility(poss, row2, col2, this))
                    wasProgressMade = true;
            }
        }
        
        return wasProgressMade;
    }
}

