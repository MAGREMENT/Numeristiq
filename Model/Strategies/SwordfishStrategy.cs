﻿using System.Collections.Generic;

namespace Model.Strategies;

public class SwordfishStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        for (int number = 1; number <= 9; number++)
        {
            Queue<PositionsAndValue> toSearch = new();
            //Rows
            for (int row = 0; row < 9; row++)
            {
                Positions p = solver.PossibilityPositionsInRow(row, number);
                if (p.Count is 2 or 3) toSearch.Enqueue(new PositionsAndValue(p, row));
            }

            while (toSearch.Count > 0)
            {
                PositionsAndValue first = toSearch.Dequeue();
                Queue<PositionsAndValue> copyOne = new Queue<PositionsAndValue>(toSearch);
                while (copyOne.Count > 0)
                {
                    PositionsAndValue second = copyOne.Dequeue();
                    Positions mashed = first.Positions.Mash(second.Positions);
                    if (mashed.Count == 3)
                    {
                        Queue<PositionsAndValue> copyTwo = new Queue<PositionsAndValue>(copyOne);
                        while (copyTwo.Count > 0)
                        {
                            PositionsAndValue third = copyTwo.Dequeue();
                            if(mashed.Mash(third.Positions).Count == 3)
                                ProcessSwordfishInRows(solver, first.Value,
                                    second.Value, third.Value, mashed, number);
                        }
                    }
                }
            }
            
            //Columns
            for (int col = 0; col < 9; col++)
            {
                Positions p = solver.PossibilityPositionsInColumn(col, number);
                if (p.Count is 2 or 3) toSearch.Enqueue(new PositionsAndValue(p, col));
            }

            while (toSearch.Count > 0)
            {
                PositionsAndValue first = toSearch.Dequeue();
                Queue<PositionsAndValue> copyOne = new Queue<PositionsAndValue>(toSearch);
                while (copyOne.Count > 0)
                {
                    PositionsAndValue second = copyOne.Dequeue();
                    Positions mashed = first.Positions.Mash(second.Positions);
                    if (mashed.Count == 3)
                    {
                        Queue<PositionsAndValue> copyTwo = new Queue<PositionsAndValue>(copyOne);
                        while (copyTwo.Count > 0)
                        {
                            PositionsAndValue third = copyTwo.Dequeue();
                            if(mashed.Mash(third.Positions).Count == 3)
                                ProcessSwordfishInColumns(solver, first.Value,
                                    second.Value, third.Value, mashed, number);
                        }
                    }
                }
            }
        }
    }

    private void ProcessSwordfishInRows(ISolver solver, int row1, int row2, int row3, Positions cols, int number)
    {
        foreach (var col in cols.All())
        {
            for (int row = 0; row < 9; row++)
            {
                if (row != row1 && row != row2 && row != row3 && solver.Possibilities[row, col].Peek(number))
                {
                    solver.RemovePossibility(number, row, col, new SwordfishLog(number, row, col, true));
                }
            }
        }
    }

    private void ProcessSwordfishInColumns(ISolver solver, int col1, int col2, int col3, Positions rows, int number)
    {
        foreach (var row in rows.All())
        {
            for (int col = 0; col < 9; col++)
            {
                if (col != col1 && col != col2 && col != col3 && solver.Possibilities[row, col].Peek(number))
                {
                    solver.RemovePossibility(number, row, col, new SwordfishLog(number, row, col, false));
                }
            }
        }
    }
}

public class PositionsAndValue
{
    public PositionsAndValue(Positions positions, int value)
    {
        Positions = positions;
        Value = value;
    }

    public Positions Positions { get; }
    public int Value { get; }
}

public class SwordfishLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public SwordfishLog(int number, int row, int col, bool isRowType)
    {
        string type = isRowType ? "row type" : "column type";
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of {type} swordfish";
    }
}