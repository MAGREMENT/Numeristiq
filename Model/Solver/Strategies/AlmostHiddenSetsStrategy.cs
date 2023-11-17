using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.Strategies;

public class AlmostHiddenSetsStrategy : AbstractStrategy
{
    public const string OfficialName = "Almost Hidden Sets";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlmostHiddenSetsStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        var allAhs = strategyManager.AlmostHiddenSetSearcher.FullGrid();
        strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;
        

        for (int i = 0; i < allAhs.Count; i++)
        {
            for (int j = i + 1; j < allAhs.Count; j++)
            {
                var one = allAhs[i];
                var two = allAhs[j];

                if (one.Possibilities.PeekAny(two.Possibilities)) continue;

                var and = one.Positions.And(two.Positions);

                switch (and.Count)
                {
                    case 2:
                        if (Process2CommonCells(strategyManager, one, two)) return;
                        continue;
                    case 1 :
                        if (Process1CommonCell(strategyManager, one, two, and, graph)) return;
                        break;
                }
            }
        }
    }

    private bool Process2CommonCells(IStrategyManager strategyManager, IPossibilitiesPositions one,
        IPossibilitiesPositions two)
    {
        foreach (var cell in one.EachCell())
        {
            if (two.Positions.Peek(cell)) continue;
                    
            foreach (var possibility in strategyManager.PossibilitiesAt(cell))
            {
                if (one.Possibilities.Peek(possibility)) continue;

                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }
                    
        foreach (var cell in two.EachCell())
        {
            if (one.Positions.Peek(cell)) continue;
                    
            foreach (var possibility in strategyManager.PossibilitiesAt(cell))
            {
                if (two.Possibilities.Peek(possibility)) continue;

                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two, new List<Link<CellPossibility>>()))
                                                       && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool Process1CommonCell(IStrategyManager strategyManager, IPossibilitiesPositions one,
        IPossibilitiesPositions two, GridPositions inCommon, LinkGraph<CellPossibility> graph)
    {
        List<Link<CellPossibility>> links = new();

        foreach (var cell in one.EachCell())
        {
            if (inCommon.Peek(cell)) continue;

            foreach (var possibility in strategyManager.PossibilitiesAt(cell))
            {
                if (one.Possibilities.Peek(possibility) || two.Possibilities.Peek(possibility)) continue;

                var current = new CellPossibility(cell, possibility);
                foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
                {
                    foreach (var friendOfFriend in graph.GetLinks(friend, LinkStrength.Strong))
                    {
                        var asCell = friendOfFriend.ToCell();

                        if (two.Positions.Peek(asCell) && !inCommon.Peek(asCell))
                        {
                            links.Add(new Link<CellPossibility>(current, friend));
                            links.Add(new Link<CellPossibility>(friend, friendOfFriend));

                            strategyManager.ChangeBuffer.ProposeSolutionAddition(friend);
                        }
                    }
                }
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                   new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two, links)) &&
                        OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class AlmostHiddenSetsAndStrongLinksReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilitiesPositions _one;
    private readonly IPossibilitiesPositions _two;
    private readonly List<Link<CellPossibility>> _links;

    public AlmostHiddenSetsAndStrongLinksReportBuilder(IPossibilitiesPositions one, IPossibilitiesPositions two, List<Link<CellPossibility>> links)
    {
        _one = one;
        _two = two;
        _links = links;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _one.EachCell())
            {
                foreach (var possibility in _one.PossibilitiesInCell(cell))
                {
                    lighter.HighlightPossibility(possibility, cell.Row, cell.Col, ChangeColoration.CauseOffOne);
                }
            }
            
            foreach (var cell in _two.EachCell())
            {
                foreach (var possibility in _two.PossibilitiesInCell(cell))
                {
                    lighter.HighlightPossibility(possibility, cell.Row, cell.Col, ChangeColoration.CauseOffTwo);
                }
                
            }

            foreach (var link in _links)
            {
                lighter.CreateLink(link.To, link.From, LinkStrength.Strong);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}