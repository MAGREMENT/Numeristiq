using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

/// <summary>
/// Naked doubles are a special case of naked possibilities where there are only 2 candidates concerned. See the doc
/// of NakedPossibilities.cs for more information.
/// </summary>
public class NakedDoublesStrategy : AbstractStrategy
{
    public const string OfficialName = "Naked Doubles";
    
    public NakedDoublesStrategy() : base(OfficialName, StrategyDifficulty.Easy){}

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<IReadOnlyPossibilities, int> dict = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pos = strategyManager.PossibilitiesAt(row, col);
                if (pos.Count != 2) continue;

                if (dict.TryGetValue(pos, out var otherCol)) ProcessRow(strategyManager, pos, row, col, otherCol);
                else dict.Add(pos, col);
            }

            dict.Clear();
        }

        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                var pos = strategyManager.PossibilitiesAt(row, col);
                if (pos.Count != 2) continue;

                if (dict.TryGetValue(pos, out var otherRow)) ProcessColumn(strategyManager, pos, col, row, otherRow);
                else dict.Add(pos, row);
            }

            dict.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                int startRow = miniRow * 3;
                int startCol = miniCol * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = startRow + gridRow;
                        int col = startCol + gridCol;

                        var pos = strategyManager.PossibilitiesAt(row, col);
                        if (pos.Count != 2) continue;

                        var gridNumber = gridRow * 3 + gridCol;
                        if (dict.TryGetValue(pos, out var otherGridNumber))
                            ProcessMiniGrid(strategyManager, pos, miniRow, miniCol, gridNumber, otherGridNumber);
                        else dict.Add(pos, gridNumber);
                    }
                }
                
                dict.Clear();
            }
        }
    }

    private void ProcessRow(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities, int row, int col1,
        int col2)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == col1 || col == col2) continue;

            foreach (var possibility in possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this,
            new LineNakedDoublesReportBuilder(possibilities, row, col1, col2, Unit.Row));
    }

    private void ProcessColumn(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities, int col,
        int row1, int row2)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == row1 || row == row2) continue;

            foreach (var possibility in possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this,
            new LineNakedDoublesReportBuilder(possibilities, col, row1, row2, Unit.Column));
    }

    private void ProcessMiniGrid(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
        int miniRow, int miniCol, int gridNumber1, int gridNumber2)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int gridNumber = gridRow * 3 + gridCol;
                if (gridNumber == gridNumber1 || gridNumber == gridNumber2) continue;

                int row = miniRow * 3 + gridRow;
                int col = miniCol * 3 + gridCol;
                foreach (var possibility in possibilities)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this,
            new MiniGridNakedDoublesReportBuilder(possibilities, miniRow, miniCol, gridNumber1, gridNumber2));
    }

}

public class LineNakedDoublesReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyPossibilities _pos;
    private readonly int _unitNumber;
    private readonly int _other1;
    private readonly int _other2;
    private readonly Unit _unit;

    public LineNakedDoublesReportBuilder(IReadOnlyPossibilities pos, int unitNumber, int other1, int other2, Unit unit)
    {
        _pos = pos;
        _unitNumber = unitNumber;
        _other1 = other1;
        _other2 = other2;
        _unit = unit;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<CellPossibility> cells = new(4);
        
        foreach (var possibility in _pos)
        {
            switch (_unit)
            {
                case Unit.Row :
                    cells.Add(new CellPossibility(_unitNumber, _other1, possibility));
                    cells.Add(new CellPossibility(_unitNumber, _other2, possibility));
                    break;
                case Unit.Column :
                    cells.Add(new CellPossibility(_other1, _unitNumber, possibility));
                    cells.Add(new CellPossibility(_other2, _unitNumber, possibility));
                    break;
            } 
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(cell, ChangeColoration.CauseOffOne);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        //TODO
        return "";
    }
}

public class MiniGridNakedDoublesReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyPossibilities _pos;
    private readonly int _miniRow;
    private readonly int _miniCol;
    private readonly int _gn1;
    private readonly int _gn2;

    public MiniGridNakedDoublesReportBuilder(IReadOnlyPossibilities pos, int miniRow, int miniCol, int gn1, int gn2)
    {
        _pos = pos;
        _miniRow = miniRow;
        _miniCol = miniCol;
        _gn1 = gn1;
        _gn2 = gn2;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<CellPossibility> cells = new(4);
        
        foreach (var possibility in _pos)
        {
            int row = _miniRow * 3 + _gn1 / 3;
            int col = _miniCol * 3 + _gn2 % 3;
            
            cells.Add(new CellPossibility(row, col, possibility));
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(cell, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private string Explanation()
    {
        //TODO
        return "";
    }
}