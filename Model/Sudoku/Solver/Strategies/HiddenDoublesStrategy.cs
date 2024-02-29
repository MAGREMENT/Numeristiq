using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.Position;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class HiddenDoublesStrategy : SudokuStrategy
{
    public const string OfficialName = "Hidden Doubles";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public HiddenDoublesStrategy() : base(OfficialName, StrategyDifficulty.Easy, DefaultBehavior){}
    
    public override void Apply(IStrategyUser strategyUser)
    {
        Dictionary<IReadOnlyLinePositions, int> lines = new();
        Dictionary<IReadOnlyMiniGridPositions, int> minis = new();
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyUser.RowPositionsAt(row, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n))
                {
                    if (ProcessRow(strategyUser, pos, row, number, n)) return;
                }
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyUser.ColumnPositionsAt(col, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n))
                {
                    if (ProcessColumn(strategyUser, pos, col, number, n)) return;
                }
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var pos = strategyUser.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (pos.Count != 2) continue;

                    if (minis.TryGetValue(pos, out var n))
                    {
                        if (ProcessMiniGrid(strategyUser, pos, number, n)) return;
                    }
                    else minis.Add(pos, number);
                }

                minis.Clear();
            }
        }
    }
    
    private bool ProcessRow(IStrategyUser strategyUser, IReadOnlyLinePositions positions, int row, int n1,
        int n2)
    {
        foreach (var col in positions)
        {
            foreach (var possibility in strategyUser.PossibilitiesAt(row, col).EnumeratePossibilities())
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }

        return strategyUser.ChangeBuffer.Commit(
            new LineHiddenDoublesReportBuilder(row, positions, n1, n2, Unit.Row)) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool ProcessColumn(IStrategyUser strategyUser, IReadOnlyLinePositions positions, int col,
        int n1, int n2)
    {
        foreach(var row in positions)
        {
            foreach (var possibility in strategyUser.PossibilitiesAt(row, col).EnumeratePossibilities())
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }

        return strategyUser.ChangeBuffer.Commit(
            new LineHiddenDoublesReportBuilder(col, positions, n1, n2, Unit.Column)) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool ProcessMiniGrid(IStrategyUser strategyUser, IReadOnlyMiniGridPositions positions, int n1, int n2)
    {
        foreach (var cell in positions)
        {
            foreach (var possibility in strategyUser.PossibilitiesAt(cell.Row, cell.Column).EnumeratePossibilities())
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
            }
        }

        return strategyUser.ChangeBuffer.Commit(
            new MiniGridHiddenDoublesReportBuilder(positions, n1, n2)) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class LineHiddenDoublesReportBuilder : IChangeReportBuilder
{
    private readonly int _unitNumber;
    private readonly IReadOnlyLinePositions _pos;
    private readonly int _n1;
    private readonly int _n2;
    private readonly Unit _unit;

    public LineHiddenDoublesReportBuilder(int unitNumber, IReadOnlyLinePositions pos, int n1, int n2, Unit unit)
    {
        _unitNumber = unitNumber;
        _pos = pos;
        _n1 = n1;
        _n2 = n2;
        _unit = unit;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        var cells = _pos.ToCellArray(_unit, _unitNumber);

        return new ChangeReport( Explanation(cells), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(_n1, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(_n2, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation(Cell[] cells)
    {
        if (cells.Length < 2) return "";

        return $"The possibilities ({_n1}, {_n2}) are limited to the cells {cells[0]}, {cells[1]} in" +
               $" {_unit.ToString().ToLower()} {_unitNumber + 1}, so any other candidates in those cells can be removed";
    }
}

public class MiniGridHiddenDoublesReportBuilder : IChangeReportBuilder
{
    private readonly IReadOnlyMiniGridPositions _pos;
    private readonly int _n1;
    private readonly int _n2;

    public MiniGridHiddenDoublesReportBuilder(IReadOnlyMiniGridPositions pos, int n1, int n2)
    {
        _pos = pos;
        _n1 = n1;
        _n2 = n2;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IPossibilitiesHolder snapshot)
    {
        var cells = _pos.ToCellArray();

        return new ChangeReport( Explanation(cells), lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(_n1, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(_n2, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private string Explanation(Cell[] cells)
    {
        if (cells.Length < 2) return "";
        
        return $"The possibilities ({_n1}, {_n2}) are limited to the cells {cells[0]}, {cells[1]} in" +
               $" mini grid {_pos.MiniGridNumber() + 1}, so any other candidates in those cells can be removed";
    }
}