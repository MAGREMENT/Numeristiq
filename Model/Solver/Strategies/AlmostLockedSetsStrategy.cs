using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class AlmostLockedSetsStrategy : AbstractStrategy //TODO add chains
{
    public const string OfficialName = "Almost Locked Sets";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlmostLockedSetsStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        var allAls = strategyManager.PreComputer.AlmostLockedSets();

        for (int i = 0; i < allAls.Count; i++)
        {
            for (int j = i + 1; j < allAls.Count; j++)
            {
                var one = allAls[i];
                var two = allAls[j];

                if (one.Positions.PeakAny(two.Positions)) continue;

                var restrictedCommons = RestrictedCommons(one, two);
                if (restrictedCommons.Count is 0 or > 2) continue;

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
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Col);
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
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Col);
                        }
                    }
                    
                    foreach (var possibility in two.Possibilities)
                    {
                        if (restrictedCommons.Peek(possibility) || one.Possibilities.Peek(possibility)) continue;

                        foreach (var coord in Cells.SharedSeenCells(new List<Cell>(two.EachCell(possibility))))
                        {
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Col);
                        }
                    }
                }

                if(strategyManager.ChangeBuffer.Commit(this, new AlmostLockedSetsReportBuilder(one,
                       two, restrictedCommons)) && OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }

    private Possibilities RestrictedCommons(IPossibilitiesPositions one, IPossibilitiesPositions two)
    {
        Possibilities result = Possibilities.NewEmpty();

        foreach (var possibility in one.Possibilities)
        {
            if (!two.Possibilities.Peek(possibility)) continue;

            if (IsPossibilityRestricted(one, two, possibility)) result.Add(possibility);
        }

        return result;
    }

    private bool IsPossibilityRestricted(IPossibilitiesPositions one, IPossibilitiesPositions two,
        int possibility)
    {
        foreach (var cell1 in one.EachCell(possibility))
        {
            foreach (var cell2 in two.EachCell(possibility))
            {
                if (!Cells.ShareAUnit(cell1, cell2)) return false;
            }
        }

        return true;
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

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in _one.EachCell())
            {
                lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            foreach (var coord in _two.EachCell())
            {
                lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffTwo);
            }

            foreach (var possibility in _restrictedCommons)
            {
                foreach (var coord in _one.EachCell())
                {
                    if(snapshot.PossibilitiesAt(coord).Peek(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Col, ChangeColoration.Neutral);
                }
                
                foreach (var coord in _two.EachCell())
                {
                    if(snapshot.PossibilitiesAt(coord).Peek(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Col, ChangeColoration.Neutral);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}