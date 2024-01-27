using System;
using System.Collections.Generic;
using Model.SudokuSolving.Solver.Position;
using Model.SudokuSolving.Solver.StrategiesUtility;

namespace Model.SudokuSolving.Solver.Strategies.MultiSector.Searchers;

public class RowsAndColumnsSearcher : ISetEquivalenceSearcher, IMultiSectorCellsSearcher
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

    private static readonly CoverHouse[] Rows =
    {
        new(Unit.Row, 0),
        new(Unit.Row, 1),
        new(Unit.Row, 2),
        new(Unit.Row, 3),
        new(Unit.Row, 4),
        new(Unit.Row, 5),
        new(Unit.Row, 6),
        new(Unit.Row, 7),
        new(Unit.Row, 8),
    };
    
    private static readonly CoverHouse[] Columns =
    {
        new(Unit.Column, 0),
        new(Unit.Column, 1),
        new(Unit.Column, 2),
        new(Unit.Column, 3),
        new(Unit.Column, 4),
        new(Unit.Column, 5),
        new(Unit.Column, 6),
        new(Unit.Column, 7),
        new(Unit.Column, 8),
    };

    public IEnumerable<GridPositions> SearchGrids(IStrategyManager strategyManager)
    {
        var possibilityGrid = new GridPositions();
        
        for (int i = 1; i <= 9; i++)
        {
            possibilityGrid = strategyManager.PositionsFor(i).Or(possibilityGrid);
        }

        for (int rowCount = _minimumUnitCount; rowCount <= _maximumUnitCount; rowCount++)
        {
            for (int colCount = _minimumUnitCount; colCount <= _maximumUnitCount; colCount++)
            {
                if(Math.Abs(rowCount - colCount) > _maximumUnitDifference) continue;

                var rowCombinations = CombinationCalculator.EveryCombinationWithSpecificCount(rowCount, Rows);
                var colCombinations = CombinationCalculator.EveryCombinationWithSpecificCount(colCount, Columns);
        
                foreach (var rows in rowCombinations)
                {
                    foreach (var cols in colCombinations)
                    {
                        var gpRow = new GridPositions();
                        var gpCol = new GridPositions();

                        foreach (var row in rows) gpRow.FillRow(row.Number);
                        foreach (var col in cols) gpCol.FillColumn(col.Number);

                        var set = gpRow.And(gpCol);
                        var supposedResult = set.And(possibilityGrid);

                        if (set.Count - supposedResult.Count > 2) continue;

                        yield return supposedResult;
                    }
                }
            }
        }
    }
}