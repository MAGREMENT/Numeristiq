using System.Collections.Generic;
using Model.Positions;

namespace Model.DeprecatedStrategies;

public class SwordfishStrategy : IStrategy
{
    public string Name { get; } = "Swordfish";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        for (int number = 1; number <= 9; number++)
        {
            Queue<ValuePositions> toSearch = new();
            //Rows
            for (int row = 0; row < 9; row++)
            {
                LinePositions p = solverView.PossibilityPositionsInRow(row, number);
                if (p.Count is 2 or 3) toSearch.Enqueue(new ValuePositions(p, row));
            }

            while (toSearch.Count > 0)
            {
                ValuePositions first = toSearch.Dequeue();
                Queue<ValuePositions> copyOne = new Queue<ValuePositions>(toSearch);
                while (copyOne.Count > 0)
                {
                    ValuePositions second = copyOne.Dequeue();
                    LinePositions mashed = first.Positions.Mash(second.Positions);
                    if (mashed.Count == 3)
                    {
                        Queue<ValuePositions> copyTwo = new Queue<ValuePositions>(copyOne);
                        while (copyTwo.Count > 0)
                        {
                            ValuePositions third = copyTwo.Dequeue();
                            if(mashed.Mash(third.Positions).Count == 3)
                                ProcessSwordfishInRows(solverView, first.Value,
                                    second.Value, third.Value, mashed, number);
                        }
                    }
                }
            }
            
            //Columns
            for (int col = 0; col < 9; col++)
            {
                LinePositions p = solverView.PossibilityPositionsInColumn(col, number);
                if (p.Count is 2 or 3) toSearch.Enqueue(new ValuePositions(p, col));
            }

            while (toSearch.Count > 0)
            {
                ValuePositions first = toSearch.Dequeue();
                Queue<ValuePositions> copyOne = new Queue<ValuePositions>(toSearch);
                while (copyOne.Count > 0)
                {
                    ValuePositions second = copyOne.Dequeue();
                    LinePositions mashed = first.Positions.Mash(second.Positions);
                    if (mashed.Count == 3)
                    {
                        Queue<ValuePositions> copyTwo = new Queue<ValuePositions>(copyOne);
                        while (copyTwo.Count > 0)
                        {
                            ValuePositions third = copyTwo.Dequeue();
                            if(mashed.Mash(third.Positions).Count == 3)
                                ProcessSwordfishInColumns(solverView, first.Value,
                                    second.Value, third.Value, mashed, number);
                        }
                    }
                }
            }
        }
    }

    private void ProcessSwordfishInRows(ISolverView solverView, int row1, int row2, int row3, LinePositions cols, int number)
    {
        foreach (var col in cols)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row != row1 && row != row2 && row != row3 && solverView.Possibilities[row, col].Peek(number))
                {
                    solverView.RemovePossibility(number, row, col, this);
                }
            }
        }
    }

    private void ProcessSwordfishInColumns(ISolverView solverView, int col1, int col2, int col3, LinePositions rows, int number)
    {
        foreach (var row in rows)
        {
            for (int col = 0; col < 9; col++)
            {
                if (col != col1 && col != col2 && col != col3 && solverView.Possibilities[row, col].Peek(number))
                {
                    solverView.RemovePossibility(number, row, col, this);
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