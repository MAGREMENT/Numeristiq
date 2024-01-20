using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.Strategies;

/// <summary>
/// If a group of all instances of N different digits in a band (aka 3 rows or 3 columns in the same 3 boxes)
/// is spread over max N+1 minirows/-columns, then the group will contain at least one unavoidable set.
/// </summary>
public class MiniUniquenessStrategy : AbstractStrategy
{
    public const string OfficialName = "Mini-Uniqueness";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public MiniUniquenessStrategy() : base(OfficialName, StrategyDifficulty.Medium, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int mini = 0; mini < 3; mini++)
        {
            var presence = Possibilities.NewEmpty();
            var availability = new LinePositions();
            var singleAvailability = new LinePositions();

            for (int col = 0; col < 9; col++)
            {
                var availabilityCount = 0;
                
                for (int r = 0; r < 3; r++)
                {
                    var row = mini * 3 + r;
                    var solved = strategyManager.Sudoku[row, col];
                    
                    if (solved == 0) availabilityCount++;
                    else presence.Add(solved);
                }

                if (availabilityCount > 0)
                {
                    availability.Add(col);
                    if (availabilityCount == 1) singleAvailability.Add(col);
                }
            }

            if (presence.Count <= 6 && availability.Count <= 9 - presence.Count + 2)
            {
                foreach (var col in singleAvailability)
                {
                    for (int r = 0; r < 3; r++)
                    {
                        var row = mini * 3 + r;
                        if (strategyManager.Sudoku[row, col] != 0) continue;

                        foreach (var p in presence)
                        {
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row, col);
                        }
                    }
                }

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new MiniUniquenessReportBuilder(mini, Unit.Row, presence)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
            }
            
            presence.RemoveAll();
            availability.Void();
            singleAvailability.Void();
            for (int row = 0; row < 9; row++)
            {
                var availabilityCount = 0;
                
                for (int c = 0; c < 3; c++)
                {
                    var col = mini * 3 + c;
                    var solved = strategyManager.Sudoku[row, col];
                    
                    if (solved == 0) availabilityCount++;
                    else presence.Add(solved);
                }

                if (availabilityCount > 0)
                {
                    availability.Add(row);
                    if (availabilityCount == 1) singleAvailability.Add(row);
                }
            }

            if (presence.Count <= 6 && availability.Count <= 9 - presence.Count + 2)
            {
                foreach (var row in singleAvailability)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        var col = mini * 3 + c;
                        if (strategyManager.Sudoku[row, col] != 0) continue;

                        foreach (var p in presence)
                        {
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, row, col);
                        }
                    }
                }

                if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                        new MiniUniquenessReportBuilder(mini, Unit.Row, presence)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }
}

public class MiniUniquenessReportBuilder : IChangeReportBuilder
{
    private readonly int _mini;
    private readonly Unit _unit;
    private readonly Possibilities _presence;

    public MiniUniquenessReportBuilder(int mini, Unit unit, Possibilities presence)
    {
        _mini = mini;
        _unit = unit;
        _presence = presence;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            foreach (var p in _presence)
            {
                for (int u = 0; u < 3; u++)
                {
                    for (int o = 0; o < 9; o++)
                    {
                        var cell = _unit == Unit.Row ? new Cell(_mini * 3 + u, o) : new Cell(o, _mini * 3 + u);
                        if (snapshot.Sudoku[cell.Row, cell.Column] == p) lighter.HighlightCell(cell, (ChangeColoration)color);
                    }
                }
                
                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}