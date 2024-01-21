using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies;

public class AlmostHiddenSetsChainStrategy : AbstractStrategy
{
    public const string OfficialName = "Almost Hidden Sets Chain";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    private readonly bool _checkLength2;
    
    public AlmostHiddenSetsChainStrategy(bool checkLength2) : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _checkLength2 = checkLength2;
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        var graph = strategyManager.PreComputer.AlmostHiddenSetGraph();
        strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink);
        var linkGraph = strategyManager.GraphManager.SimpleLinkGraph;

        foreach (var start in graph)
        {
            if (Search(strategyManager, graph, start.Possibilities, new HashSet<IPossibilitiesPositions> {start},
                    new ChainBuilder<IPossibilitiesPositions, Cell>(start), linkGraph)) return;
        }
    }

    private bool Search(IStrategyManager strategyManager, PositionsGraph<IPossibilitiesPositions> graph,
        IReadOnlyPossibilities occupied, HashSet<IPossibilitiesPositions> explored, ChainBuilder<IPossibilitiesPositions, Cell> chain,
        ILinkGraph<CellPossibility> linkGraph)
    {
        foreach (var friend in graph.GetLinks(chain.LastElement()))
        {
            /*if (chain.Count > 2 && chain.FirstElement().Equals(friend.To) &&
                 CheckForLoop(strategyManager, chain, friend.Cells)) return true;*/
            
            if (explored.Contains(friend.To) || occupied.PeekAny(friend.To.Possibilities)) continue;

            var lastLink = chain.LastLink();
            foreach (var possibleLink in friend.Cells)
            {
                if (chain.Count > 0 && lastLink == possibleLink) continue;

                chain.Add(possibleLink, friend.To);
                explored.Add(friend.To);
                var occupiedCopy = occupied.Or(friend.To.Possibilities);

                if (CheckForChain(strategyManager, chain, linkGraph)) return true;
                if(Search(strategyManager, graph, occupiedCopy, explored, chain, linkGraph)) return true;
                
                chain.RemoveLast();
            }
        }

        return false;
    }

    private bool CheckForLoop(IStrategyManager strategyManager, ChainBuilder<IPossibilitiesPositions, Cell> builder,
        Cell[] possibleLastLinks)
    {
        foreach (var ll in possibleLastLinks)
        {
            if (ll.Equals(builder.FirstLink())) continue;
            var chain = builder.ToChain();

            foreach (var pp in chain.Elements)
            {
                foreach (var cell in pp.EachCell())
                {
                    foreach (var p in strategyManager.PossibilitiesAt(cell))
                    {
                        var cp = new CellPossibility(cell, p);
                        if (!Contains(chain, cp)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(cp);
                    }
                }
            }

            return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                new AlmostHiddenSetsChainReportBuilder(chain, ll)) && OnCommitBehavior == OnCommitBehavior.Return;
        }

        return false;
    }

    private bool Contains(Chain<IPossibilitiesPositions, Cell> chain, CellPossibility cp)
    {
        foreach (var element in chain)
        {
            if (element.Contains(cp)) return true;
        }

        return false;
    }

    private bool CheckForChain(IStrategyManager strategyManager, ChainBuilder<IPossibilitiesPositions, Cell> chain, ILinkGraph<CellPossibility> linkGraph)
    {
        if (!_checkLength2 && chain.Count == 2) return false;
        
        List<Link<CellPossibility>> links = new();

        var first = chain.FirstElement();
        var last = chain.LastElement();

        var nope = new GridPositions();
        nope.Add(chain.FirstLink());
        nope.Add(chain.LastLink());

        foreach (var cell in first.EachCell())
        {
            if (nope.Peek(cell)) continue;
            
            foreach (var possibility in strategyManager.PossibilitiesAt(cell))
            {
                if (first.Possibilities.Peek(possibility) || last.Possibilities.Peek(possibility)) continue;

                var current = new CellPossibility(cell, possibility);
                foreach (var friend in linkGraph.Neighbors(current, LinkStrength.Strong))
                {
                    if (nope.Peek(friend.ToCell())) continue;
                    
                    foreach (var friendOfFriend in linkGraph.Neighbors(friend, LinkStrength.Strong))
                    {
                        var asCell = friendOfFriend.ToCell();

                        if (last.Positions.Peek(friendOfFriend.ToCell()) && asCell != cell && !nope.Peek(asCell))
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
            new AlmostHiddenSetsChainReportBuilder(chain.ToChain(), links)) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class AlmostHiddenSetsChainReportBuilder : IChangeReportBuilder
{
    private readonly Chain<IPossibilitiesPositions, Cell> _chain;
    private readonly List<Link<CellPossibility>> _links;
    private readonly Cell? _additionalLink;

    public AlmostHiddenSetsChainReportBuilder(Chain<IPossibilitiesPositions, Cell> chain, List<Link<CellPossibility>> links)
    {
        _chain = chain;
        _links = links;
        _additionalLink = null;
    }

    public AlmostHiddenSetsChainReportBuilder(Chain<IPossibilitiesPositions, Cell> chain, Cell additionalLink)
    {
        _chain = chain;
        _links = new List<Link<CellPossibility>>();
        _additionalLink = additionalLink;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), _chain.ToString(), lighter =>
        {
            var color = (int)ChangeColoration.CauseOffOne;
            foreach (var ahs in _chain.Elements)
            {
                foreach (var possibility in ahs.Possibilities)
                {
                    foreach (var cell in ahs.EachCell(possibility))
                    {
                        lighter.HighlightPossibility(possibility, cell.Row, cell.Column, (ChangeColoration)color);
                    }
                }

                color++;
            }

            foreach (var cell in _chain.Links)
            {
                lighter.EncircleCell(cell);
            }
            if(_additionalLink is not null) lighter.EncircleCell(_additionalLink.Value);

            foreach (var link in _links)
            {
                lighter.CreateLink(link.From, link.To, LinkStrength.Strong);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}