using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Sudoku.Solver.Strategies;

public class NakedDoublesStrategy : AbstractStrategy
{
    public const string OfficialName = "Naked Doubles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public NakedDoublesStrategy() : base(OfficialName, StrategyDifficulty.Easy, DefaultBehavior){}

    public override void Apply(IStrategyManager strategyManager)
    {
        Dictionary<IReadOnlyPossibilities, int> dict = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pos = strategyManager.PossibilitiesAt(row, col);
                if (pos.Count != 2) continue;

                if (dict.TryGetValue(pos, out var otherCol))
                {
                    if (ProcessRow(strategyManager, pos, row, col, otherCol)) return;
                }
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

                if (dict.TryGetValue(pos, out var otherRow))
                {
                    if (ProcessColumn(strategyManager, pos, col, row, otherRow)) return;
                }
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
                        {
                            if (ProcessMiniGrid(strategyManager, pos, miniRow, miniCol, gridNumber, otherGridNumber))
                                return;
                        }
                        else dict.Add(pos, gridNumber);
                    }
                }
                
                dict.Clear();
            }
        }
    }

    private bool ProcessRow(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities, int row, int col1,
        int col2)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == col1 || col == col2) continue;

            foreach (var possibility in possibilities)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }

        return strategyManager.ChangeBuffer.Commit(this,
            new LineNakedDoublesReportBuilder(possibilities, row, col1, col2, Unit.Row))
            && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool ProcessColumn(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities, int col,
        int row1, int row2)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == row1 || row == row2) continue;

            foreach (var possibility in possibilities)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }

        return strategyManager.ChangeBuffer.Commit(this,
            new LineNakedDoublesReportBuilder(possibilities, col, row1, row2, Unit.Column))
            && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool ProcessMiniGrid(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
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
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
                }
            }
        }

        return strategyManager.ChangeBuffer.Commit(this,
            new MiniGridNakedDoublesReportBuilder(possibilities, miniRow, miniCol, gridNumber1, gridNumber2))
            && OnCommitBehavior == OnCommitBehavior.Return;
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport( Explanation(), lighter =>
        {
            foreach (var possibility in _pos)
            {
                switch (_unit)
                {
                    case Unit.Row :
                        lighter.HighlightPossibility(possibility, _unitNumber, _other1, ChangeColoration.CauseOffOne);
                        lighter.HighlightPossibility(possibility, _unitNumber, _other2, ChangeColoration.CauseOffOne);
                        break;
                    case Unit.Column :
                        lighter.HighlightPossibility(possibility, _other1, _unitNumber, ChangeColoration.CauseOffOne);
                        lighter.HighlightPossibility(possibility, _other2, _unitNumber, ChangeColoration.CauseOffOne);
                        break;
                } 
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        string cells = _unit == Unit.Row
            ? $"[{_unitNumber}, {_other1}], [{_unitNumber}, {_other2}]"
            : $"[{_other1}, {_unitNumber}], [{_other2}, {_unitNumber}]";
        return $"The cells {cells} only contains the possibilities ({_pos})." +
               $" Any other cell in {_unit.ToString().ToLower()} {_unitNumber + 1} cannot contain these possibilities";
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<CellPossibility> cells = new(4);
        
        foreach (var possibility in _pos)
        {
            cells.Add(new CellPossibility(_miniRow * 3 + _gn1 / 3, _miniCol * 3 + _gn1 % 3, possibility));
            cells.Add(new CellPossibility(_miniRow * 3 + _gn2 / 3, _miniCol * 3 + _gn2 % 3, possibility));
        }
        
        return new ChangeReport( Explanation(cells), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(cell, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private string Explanation(IReadOnlyList<CellPossibility> cells)
    {
        return $"The cells {cells[0]}, {cells[1]} only contains the possibilities ({_pos}). Any other cell in" +
               $" mini grid {_miniRow * 3 + _miniCol + 1} cannot contain these possibilities";
    }
}