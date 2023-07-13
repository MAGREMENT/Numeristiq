using System.Collections.Generic;

namespace Model.Strategies;

public class XWingStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        Dictionary<Positions, int> dict = new();
        for (int n = 1; n <= 9; n++)
        {
            //Rows
            dict.Clear();
            for (int row = 0; row < 9; row++)
            {
                var ppir = solver.PossibilityPositionsInRow(row, n);
                if (ppir.Count == 2)
                {
                    if (!dict.TryAdd(ppir, row))
                    {
                        RemoveFromColumns(solver, ppir.All(), dict[ppir], row, n);
                    }
                }
            }
            
            //Columns
            dict.Clear();
            for (int col = 0; col < 9; col++)
            {
                var ppic = solver.PossibilityPositionsInColumn(col, n);
                if (ppic.Count == 2)
                {
                    if (!dict.TryAdd(ppic, col))
                    {
                        RemoveFromRows(solver, ppic.All(), dict[ppic], col, n);
                    }
                }
            }
        }
    }

    private void RemoveFromColumns(ISolver solver, IEnumerable<int> cols, int row1, int row2, int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row != row1 && row != row2)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var col in cols)
                {
                    solver.RemovePossibility(number, row, col, new XWingLog(number, row, col));
                }
            }
        }
    }

    private void RemoveFromRows(ISolver solver, IEnumerable<int> rows, int col1, int col2, int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col != col1 && col != col2)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var row in rows)
                {
                    solver.RemovePossibility(number, row, col, new XWingLog(number, row, col));
                }
            }
        }
    }
}

public class XWingLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public XWingLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of X-Wings";
    }
}