using System.Collections.Generic;
using Model.Positions;
using Model.Solver;

namespace Model.DeprecatedStrategies;

public class SwordfishStrategy : IStrategy
{
    public string Name { get; } = "Swordfish";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            Queue<ValuePositions> toSearch = new();
            //Rows
            for (int row = 0; row < 9; row++)
            {
                LinePositions p = strategyManager.RowPositions(row, number);
                if (p.Count is 2 or 3) toSearch.Enqueue(new ValuePositions(p, row));
            }

            while (toSearch.Count > 0)
            {
                ValuePositions first = toSearch.Dequeue();
                Queue<ValuePositions> copyOne = new Queue<ValuePositions>(toSearch);
                while (copyOne.Count > 0)
                {
                    ValuePositions second = copyOne.Dequeue();
                    LinePositions mashed = first.Positions.Or(second.Positions);
                    if (mashed.Count == 3)
                    {
                        Queue<ValuePositions> copyTwo = new Queue<ValuePositions>(copyOne);
                        while (copyTwo.Count > 0)
                        {
                            ValuePositions third = copyTwo.Dequeue();
                            if(mashed.Or(third.Positions).Count == 3)
                                ProcessSwordfishInRows(strategyManager, first.Value,
                                    second.Value, third.Value, mashed, number);
                        }
                    }
                }
            }
            
            //Columns
            for (int col = 0; col < 9; col++)
            {
                LinePositions p = strategyManager.ColumnPositions(col, number);
                if (p.Count is 2 or 3) toSearch.Enqueue(new ValuePositions(p, col));
            }

            while (toSearch.Count > 0)
            {
                ValuePositions first = toSearch.Dequeue();
                Queue<ValuePositions> copyOne = new Queue<ValuePositions>(toSearch);
                while (copyOne.Count > 0)
                {
                    ValuePositions second = copyOne.Dequeue();
                    LinePositions mashed = first.Positions.Or(second.Positions);
                    if (mashed.Count == 3)
                    {
                        Queue<ValuePositions> copyTwo = new Queue<ValuePositions>(copyOne);
                        while (copyTwo.Count > 0)
                        {
                            ValuePositions third = copyTwo.Dequeue();
                            if(mashed.Or(third.Positions).Count == 3)
                                ProcessSwordfishInColumns(strategyManager, first.Value,
                                    second.Value, third.Value, mashed, number);
                        }
                    }
                }
            }
        }
    }

    private void ProcessSwordfishInRows(IStrategyManager strategyManager, int row1, int row2, int row3, LinePositions cols, int number)
    {
        foreach (var col in cols)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row != row1 && row != row2 && row != row3 && strategyManager.Possibilities[row, col].Peek(number))
                {
                    strategyManager.RemovePossibility(number, row, col, this);
                }
            }
        }
    }

    private void ProcessSwordfishInColumns(IStrategyManager strategyManager, int col1, int col2, int col3, LinePositions rows, int number)
    {
        foreach (var row in rows)
        {
            for (int col = 0; col < 9; col++)
            {
                if (col != col1 && col != col2 && col != col3 && strategyManager.Possibilities[row, col].Peek(number))
                {
                    strategyManager.RemovePossibility(number, row, col, this);
                }
            }
        }
    }
}

public class ValuePositions
{
    public ValuePositions(LinePositions positions, int value)
    {
        Positions = positions;
        Value = value;
    }

    public LinePositions Positions { get; }
    public int Value { get; }
}