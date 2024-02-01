using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.PossibilityPosition;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class AlmostLockedSetsStrategy : AbstractStrategy
{
    public const string OfficialName = "Almost Locked Sets";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlmostLockedSetsStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        foreach (var linked in strategyManager.PreComputer.ConstructAlmostLockedSetGraph())
        {
            if (linked.RestrictedCommons.Count > 2) continue;

            var restrictedCommons = linked.RestrictedCommons;
            var one = linked.One;
            var two = linked.Two;
            
            foreach (var restrictedCommon in restrictedCommons)
            {
                foreach (var possibility in one.Possibilities)
                {
                    if (!two.Possibilities.Peek(possibility) || possibility == restrictedCommon) continue;

                    List<Cell> coords = new();
                    coords.AddRange(one.EachCell(possibility));
                    coords.AddRange(two.EachCell(possibility));

                    foreach (var coord in Cells.SharedSeenCells(coords))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if (restrictedCommons.Count == 2)
            {
                foreach (var possibility in one.Possibilities)
                {
                    if (restrictedCommons.Peek(possibility) || two.Possibilities.Peek(possibility)) continue;

                    foreach (var coord in Cells.SharedSeenCells(new List<Cell>(one.EachCell(possibility))))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
                    
                foreach (var possibility in two.Possibilities)
                {
                    if (restrictedCommons.Peek(possibility) || one.Possibilities.Peek(possibility)) continue;

                    foreach (var coord in Cells.SharedSeenCells(new List<Cell>(two.EachCell(possibility))))
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if(strategyManager.ChangeBuffer.Commit(this, new AlmostLockedSetsReportBuilder(one,
                   two, restrictedCommons)) && OnCommitBehavior == OnCommitBehavior.Return) return;
        }
    }
}

public class AlmostLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilitiesPositions _one;
    private readonly IPossibilitiesPositions _two;
    private readonly Possibilities _restrictedCommons;

    public AlmostLockedSetsReportBuilder(IPossibilitiesPositions one, IPossibilitiesPositions two, Possibilities restrictedCommons)
    {
        _one = one;
        _two = two;
        _restrictedCommons = restrictedCommons;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in _one.EachCell())
            {
                lighter.HighlightCell(coord.Row, coord.Column, ChangeColoration.CauseOffOne);
            }

            foreach (var coord in _two.EachCell())
            {
                lighter.HighlightCell(coord.Row, coord.Column, ChangeColoration.CauseOffTwo);
            }

            foreach (var possibility in _restrictedCommons)
            {
                foreach (var coord in _one.EachCell())
                {
                    if(snapshot.PossibilitiesAt(coord).Peek(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, ChangeColoration.Neutral);
                }
                
                foreach (var coord in _two.EachCell())
                {
                    if(snapshot.PossibilitiesAt(coord).Peek(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, ChangeColoration.Neutral);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}