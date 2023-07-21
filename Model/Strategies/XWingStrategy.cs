using System.Collections.Generic;
using Model.Positions;

namespace Model.Strategies;

public class XWingStrategy : IStrategy
{
    public string Name { get; } = "XWing";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        Dictionary<LinePositions, int> dict = new();
        for (int n = 1; n <= 9; n++)
        {
            //Rows
            dict.Clear();
            for (int row = 0; row < 9; row++)
            {
                var ppir = solverView.PossibilityPositionsInRow(row, n);
                if (ppir.Count == 2)
                {
                    if (!dict.TryAdd(ppir, row))
                    {
                        RemoveFromColumns(solverView, ppir, dict[ppir], row, n);
                    }
                }
            }
            
            //Columns
            dict.Clear();
            for (int col = 0; col < 9; col++)
            {
                var ppic = solverView.PossibilityPositionsInColumn(col, n);
                if (ppic.Count == 2)
                {
                    if (!dict.TryAdd(ppic, col))
                    {
                        RemoveFromRows(solverView, ppic, dict[ppic], col, n);
                    }
                }
            }
        }
    }

    private void RemoveFromColumns(ISolverView solverView, IEnumerable<int> cols, int row1, int row2, int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row != row1 && row != row2)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var col in cols)
                {
                    solverView.RemovePossibility(number, row, col, this);
                }
            }
        }
    }

    private void RemoveFromRows(ISolverView solverView, IEnumerable<int> rows, int col1, int col2, int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col != col1 && col != col2)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var row in rows)
                {
                    solverView.RemovePossibility(number, row, col, this);
                }
            }
        }
    }
}