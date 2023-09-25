using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;
using Model.StrategiesUtil;

namespace Model.Solver.Strategies;

/// <summary>
/// A X-wing is when 2 row/column have the same possibility in only 2 of their cells and in the same column/row, forming
/// an X or a square. When that happens, the cells in the columns or rows that aren't in the X-wing cannot contain that
/// possibility.
///
/// Example :
///
/// +-------+-------+-------+
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . x | . x x | . . . |
/// +-------+-------+-------+
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// +-------+-------+-------+
/// | x . x | . . x | . . . |
/// | . . . | . . . | x . . |
/// | . . . | . . . | . . . |
/// +-------+-------+-------+
///
/// If we consider the x-marked cells to be the same possibility, the wing pattern is in column 3 & 6. We can remove the
/// x-marked cells in row 3 & 7 that arent in the X, so [3, 4] & [7, 1].
/// 
/// </summary>
public class XWingStrategy : IStrategy
{
    public string Name => "XWing";
    
    public StrategyDifficulty Difficulty => StrategyDifficulty.Medium;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<IReadOnlyLinePositions, int> dict = new();
        for (int n = 1; n <= 9; n++)
        {
            //Rows
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, n);
                if (ppir.Count != 2) continue;
                
                if (!dict.TryAdd(ppir, row))
                {
                    RemoveFromColumns(strategyManager, ppir, dict[ppir], row, n);
                }
            }
            dict.Clear();
            
            //Columns
            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.ColumnPositionsAt(col, n);
                if (ppic.Count != 2) continue;
                
                if (!dict.TryAdd(ppic, col))
                {
                    RemoveFromRows(strategyManager, ppic, dict[ppic], col, n);
                }
            }
            dict.Clear();
        }
    }

    private void RemoveFromColumns(IStrategyManager strategyManager, IReadOnlyLinePositions cols, int row1, int row2, int number)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == row1 || row == row2) continue;
            
            foreach (var col in cols)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
            }
        }
        
        strategyManager.ChangeBuffer.Push(this, new XWingReportBuilder(cols, row1, row2, number, Unit.Row));
    }

    private void RemoveFromRows(IStrategyManager strategyManager, IReadOnlyLinePositions rows, int col1, int col2, int number)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == col1 || col == col2) continue;
            
            foreach (var row in rows)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
            }
        }
        
        strategyManager.ChangeBuffer.Push(this, new XWingReportBuilder(rows, col1, col2, number, Unit.Column));
    }
}

public class XWingReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyLinePositions _linePos;
    private readonly int _unit1;
    private readonly int _unit2;
    private readonly int _number;
    private readonly Unit _unit;

    public XWingReportBuilder(IReadOnlyLinePositions linePos, int unit1, int unit2, int number, Unit unit)
    {
        _linePos = linePos;
        _unit1 = unit1;
        _unit2 = unit2;
        _number = number;
        _unit = unit;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> cells = new();
        foreach (var other in _linePos)
        {
            switch (_unit)
            {
                case Unit.Row :
                    cells.Add(new Cell(_unit1, other));
                    cells.Add(new Cell(_unit2, other));
                    break;
                case Unit.Column :
                    cells.Add(new Cell(other, _unit1));
                    cells.Add(new Cell(other, _unit2));
                    break;
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(cells), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(_number, cell.Row, cell.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation(List<Cell> cells)
    {
        var otherUnit = _unit == Unit.Row ? "column" : "row";
        int cursor = -1;
        var other1 = _linePos.Next(ref cursor);
        var other2 = _linePos.Next(ref cursor);
        return $"The cells ({cells[0]}, {cells[1]}, {cells[2]} & {cells[3]}) form an X-wing. It means that whichever" +
               $" cell is the solution for {_number} in {_unit.ToString().ToLower()}s {_unit1 + 1} & {_unit2 + 1}, {_number} will" +
               $" be in those cells for the {otherUnit}s {other1 + 1} & {other2 + 1}. Therefor, any cells in those {otherUnit}s" +
               $" not in the x-wing cannot contain {_number}";
    }
}