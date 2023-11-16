using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;
using Model.Solver.StrategiesUtil;

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

        for (int n1 = 1; n1 <= 9; n1++)
        {
            var pos1 = positions[n1 - 1];
            if(pos1.Count is > 2 or < 1) continue;
            
            for (int n2 = 1; n2 <= 9; n2++)
            {
                var pos2 = positions[n2 - 1];
                if (pos2.Count is > 2 or < 1) continue;

                var or = pos1.Or(pos2);
                if(or.Count is >= 4 or <= 1) continue;

                if (Process(strategyManager, n1, n2, or)) return;
            }
        }
    }

    private bool Process(IStrategyManager strategyManager, int n1, int n2, GridPositions or)
    {
        LinePositions rows = new LinePositions();
        LinePositions cols = new LinePositions();

        foreach (var pos in or)
        {
            rows.Add(pos.Row);
            cols.Add(pos.Col);
        }

        if (rows.Count != 2 || cols.Count != 2 || (!rows.AreAllInSameMiniGrid() && !cols.AreAllInSameMiniGrid())) return false;

        foreach (var row in rows)
        {
            foreach (var col in cols)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(n1, row, col);
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(n2, row, col);
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new ReverseBugReportBuilder(rows, cols)) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class ReverseBugReportBuilder : IChangeReportBuilder
{
    private readonly LinePositions _rows;
    private readonly LinePositions _cols;

    public ReverseBugReportBuilder(LinePositions rows, LinePositions cols)
    {
        _rows = rows;
        _cols = cols;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> cells = new(4);
        foreach (var row in _rows)
        {
            foreach (var col in _cols)
            {
                if (snapshot.PossibilitiesAt(row, col).Count == 0) cells.Add(new Cell(row, col));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}