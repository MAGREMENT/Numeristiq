using System.Collections.Generic;
using Model.Positions;

namespace Model.Strategies;

public class XWingStrategy : IStrategy
{
    public string Name => "XWing";
    
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<LinePositions, int> dict = new();
        for (int n = 1; n <= 9; n++)
        {
            //Rows
            dict.Clear();
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.PossibilityPositionsInRow(row, n);
                if (ppir.Count == 2)
                {
                    if (!dict.TryAdd(ppir, row))
                    {
                        RemoveFromColumns(strategyManager, ppir, dict[ppir], row, n);
                    }
                }
            }
            
            //Columns
            dict.Clear();
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.PossibilityPositionsInColumn(col, n);
                if (ppic.Count == 2)
                {
                    if (!dict.TryAdd(ppic, col))
                    {
                        RemoveFromRows(strategyManager, ppic, dict[ppic], col, n);
                    }
                }
            }
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        return "";
    }

    private void RemoveFromColumns(IStrategyManager strategyManager, LinePositions cols, int row1, int row2, int number)
    {
        var changeBuffer = 
            strategyManager.CreateChangeBuffer(this, new RowXWingCauseFactory(cols, row1, row2, number));
        
        for (int row = 0; row < 9; row++)
        {
            if (row != row1 && row != row2)
            {
                foreach (var col in cols)
                {
                    changeBuffer.AddPossibilityToRemove(number, row, col);
                }
            }
        }
        
        changeBuffer.Push();
    }

    private void RemoveFromRows(IStrategyManager strategyManager, LinePositions rows, int col1, int col2, int number)
    {
        var changeBuffer =
            strategyManager.CreateChangeBuffer(this, new ColumnXWingCauseFactory(rows, col1, col2, number));
        
        for (int col = 0; col < 9; col++)
        {
            if (col != col1 && col != col2)
            {
                foreach (var row in rows)
                {
                    changeBuffer.AddPossibilityToRemove(number, row, col);
                }
            }
        }
        
        changeBuffer.Push();
    }
}