using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Solver;

namespace Model.Strategies;

public class XWingStrategy : IStrategy
{
    public string Name => "XWing";
    
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<LinePositions, int> dict = new();
        for (int n = 1; n <= 9; n++)
        {
            //Rows
            dict.Clear();
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, n);
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
                var ppic = strategyManager.ColumnPositionsAt(col, n);
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

    private void RemoveFromColumns(IStrategyManager strategyManager, LinePositions cols, int row1, int row2, int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row != row1 && row != row2)
            {
                foreach (var col in cols)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
                }
            }
        }
        
        strategyManager.ChangeBuffer.Push(this, new XWingReportBuilder(cols, row1, row2, number, Unit.Row));
    }

    private void RemoveFromRows(IStrategyManager strategyManager, LinePositions rows, int col1, int col2, int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col != col1 && col != col2)
            {
                foreach (var row in rows)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
                }
            }
        }
        
        strategyManager.ChangeBuffer.Push(this, new XWingReportBuilder(rows, col1, col2, number, Unit.Column));
    }
}

public class XWingReportBuilder : IChangeReportBuilder
{
    private readonly LinePositions _linePos;
    private readonly int _unit1;
    private readonly int _unit2;
    private readonly int _number;
    private readonly Unit _unit;

    public XWingReportBuilder(LinePositions linePos, int unit1, int unit2, int number, Unit unit)
    {
        _linePos = linePos;
        _unit1 = unit1;
        _unit2 = unit2;
        _number = number;
        _unit = unit;
    }
    
    public ChangeReport Build(List<SolverChange> changes, ISolver snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var other in _linePos)
            {
                switch (_unit)
                {
                    case Unit.Row :
                        lighter.HighlightPossibility(_number, _unit1, other, ChangeColoration.CauseOffOne);
                        lighter.HighlightPossibility(_number, _unit2, other, ChangeColoration.CauseOffOne);
                        break;
                    case Unit.Column :
                        lighter.HighlightPossibility(_number, other, _unit1, ChangeColoration.CauseOffOne);
                        lighter.HighlightPossibility(_number, other, _unit2, ChangeColoration.CauseOffOne);
                        break;
                }
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}