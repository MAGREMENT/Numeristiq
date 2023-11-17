using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;

namespace Model.Solver.Strategies;

/// <summary>
/// http://forum.enjoysudoku.com/the-reverse-bug-t4431.html
/// </summary>
public class ReverseBUGStrategy : AbstractStrategy
{
    public const string OfficialName = "Reverse BUG";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public ReverseBUGStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        GridPositions[] positions = { new(), new(), new(), new(), new(), new(), new(), new(), new() };

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solution = strategyManager.Sudoku[row, col];
                if (solution != 0) positions[solution - 1].Add(row, col);
            }
        }

        for (int n1 = 1; n1 <= 8; n1++)
        {
            var pos1 = positions[n1 - 1];

            for (int n2 = 1; n2 <= 9; n2++)
            {
                var pos2 = positions[n2 - 1];
                var or = pos1.Or(pos2);
                if (or.Count >= 17) continue;

                var soloRow = CheckForSoloRow(or);
                if (soloRow == -1) continue;

                var soloCol = CheckForSoloColumn(or);
                if (soloCol == -1) continue;

                var soloMini = CheckForSoloMini(or);
                if (soloMini == -1) continue;

                var miniRow = soloMini / 3;
                var miniCol = soloCol / 3;

                if (soloRow / 3 == miniRow && soloCol / 3 == miniCol)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(n1, soloRow, soloCol);
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(n2, soloRow, soloCol);
                }

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new ReverseBugReportBuilder(or, n1)) && OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }

    private static int CheckForSoloRow(GridPositions gp)
    {
        var result = -1;

        for (int row = 0; row < 9; row++)
        {
            var count = gp.RowCount(row);
            if (count is 0 or 2) continue;

            if (count == 1)
            {
                if (result == -1) result = row;
                else return -1;
            }
        }

        return result;
    }
    
    private static int CheckForSoloColumn(GridPositions gp)
    {
        var result = -1;

        for (int col = 0; col < 9; col++)
        {
            var count = gp.ColumnCount(col);
            if (count is 0 or 2) continue;

            if (count == 1)
            {
                if (result == -1) result = col;
                else return -1;
            }
        }

        return result;
    }

    private static int CheckForSoloMini(GridPositions gp)
    {
        var result = -1;

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                var count = gp.MiniGridCount(miniRow, miniCol);
                if (count is 0 or 2) continue;

                if (count == 1)
                {
                    if (result == -1) result = miniRow * 3 + miniCol;
                    else return -1;
                }
            }
        }

        return result;
    }
}

public class ReverseBugReportBuilder : IChangeReportBuilder
{
    private readonly GridPositions _gp;
    private readonly int _n1;

    public ReverseBugReportBuilder(GridPositions gp, int n1)
    {
        _gp = gp;
        _n1 = n1;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _gp)
            {
                lighter.HighlightCell(cell.Row, cell.Col, snapshot.Sudoku[cell.Row, cell.Col] == _n1
                        ? ChangeColoration.CauseOffOne
                        : ChangeColoration.CauseOffTwo);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}