using System;
using System.Collections.Generic;
using Model.Solver.Position;

namespace Model.Solver.Strategies.SetEquivalence.Searchers;

public class RowsAndColumnsSearcher : ISetEquivalenceSearcher
{
    private readonly int _minimumUnitCount;
    private readonly int _maximumUnitCount;
    private readonly int _maximumUnitDifference;

    public RowsAndColumnsSearcher(int minimumUnitCount, int maximumUnitCount, int maximumUnitDifference)
    {
        _minimumUnitCount = minimumUnitCount;
        _maximumUnitCount = maximumUnitCount;
        _maximumUnitDifference = maximumUnitDifference;
    }

    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager)
    {
        for (int rowCount = _minimumUnitCount; rowCount <= _maximumUnitCount; rowCount++)
        {
            for (int colCount = _minimumUnitCount; colCount <= _maximumUnitCount; colCount++)
            {
                if(Math.Abs(rowCount - colCount) > _maximumUnitDifference) continue;

                var rowCombinations = EachCombinations(rowCount);
                var colCombinations = EachCombinations(colCount);
        
                foreach (var rows in rowCombinations)
                {
                    foreach (var cols in colCombinations)
                    {
                        var gpRow = new GridPositions();
                        var gpCol = new GridPositions();

                        foreach (var row in rows) gpRow.FillRow(row);
                        foreach (var col in cols) gpCol.FillColumn(col);

                        yield return new SetEquivalence(gpRow.Difference(gpCol).ToArray(), rowCount,
                            gpCol.Difference(gpRow).ToArray(), colCount);
                    }
                }
            }
        }
    }
    
    private List<LinePositions> EachCombinations(int number)
    {
        List<LinePositions> result = new();

        EachCombinations(result, new LinePositions(),number, 0, 0);

        return result;
    }

    private void EachCombinations(List<LinePositions> result, LinePositions current, int number, int count, int start)
    {
        for (int i = start; i < 9; i++)
        {
            current.Add(i);
            
            if (count + 1 == number) result.Add(current.Copy());
            else EachCombinations(result, current, number, count + 1, i + 1);

            current.Remove(i);
        }
    }
}