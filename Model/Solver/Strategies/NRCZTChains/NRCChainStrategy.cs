using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;
using Model.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Solver.Strategies.NRCZTChains;

public class NRCChainStrategy : AbstractStrategy
{
    public const string OfficialName = "NRC-Chains";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.ChooseBest;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public NRCChainStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink,
            ConstructRule.CellStrongLink, ConstructRule.CellWeakLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;

        var chain = new BlockChain();

        foreach (var start in graph)
        {
            HashSet<CellPossibility> startVisited = new();
            HashSet<CellPossibility> endVisited = new();
            startVisited.Add(start);
            
            foreach (var friend in graph.GetLinks(start, LinkStrength.Strong))
            {
                if (start == friend) continue;

                endVisited.Add(friend);
                var block = new Block(start, friend);
                chain.Add(block);

                if (Search(strategyManager, graph, startVisited, endVisited, chain)) return;
                
                chain.RemoveLast();
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, LinkGraph<CellPossibility> graph,
        HashSet<CellPossibility> startVisited, HashSet<CellPossibility> endVisited, BlockChain chain)
    {
        foreach(var bStart in graph.GetLinks(chain.Last().End))
        {
            if (startVisited.Contains(bStart)) continue;

            startVisited.Add(bStart);
            foreach (var bEnd in graph.GetLinks(bStart, LinkStrength.Strong))
            {
                if (endVisited.Contains(bEnd) || bStart == bEnd || bEnd == chain[0].Start) continue;

                endVisited.Add(bEnd);
                var block = new Block(bStart, bEnd);
                chain.Add(block);

                if (Check(strategyManager, chain)) return true;
                if (Search(strategyManager, graph, startVisited, endVisited, chain)) return true;
                
                chain.RemoveLast();
            }
        }

        return false;
    }

    private bool Check(IStrategyManager strategyManager, BlockChain chain)
    {
        var first = chain[0].Start;
        var last = chain.Last().End;

        if (first.Row == last.Row && first.Col == last.Col)
        {
            var every = chain.AllCellPossibilities();

            foreach (var possibility in strategyManager.PossibilitiesAt(first.Row, first.Col))
            {
                var current = new CellPossibility(first.Row, first.Col, possibility);
                if (every.Contains(current)) continue;

                strategyManager.ChangeBuffer.ProposePossibilityRemoval(current);
            }
        }
        else if (first.Possibility == last.Possibility && Cells.ShareAUnit(first.ToCell(), last.ToCell()))
        {
            var every = chain.AllCellPossibilities();
            foreach (var cell in Cells.SharedSeenCells(first.ToCell(), last.ToCell()))
            {
                var current = new CellPossibility(cell, first.Possibility);
                if (every.Contains(current)) continue;

                strategyManager.ChangeBuffer.ProposePossibilityRemoval(current);
            }
        }
        else return false;

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new NRCChainReportBuilder(chain.Copy())) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class NRCChainReportBuilder : IChangeReportBuilder
{
    private readonly BlockChain _chain;

    public NRCChainReportBuilder(BlockChain chain)
    {
        _chain = chain;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            for (int i = 0; i < _chain.Count; i++)
            {
                var current = _chain[i];
                
                lighter.HighlightPossibility(current.Start, ChangeColoration.CauseOnOne);
                lighter.HighlightPossibility(current.End, ChangeColoration.CauseOffOne);
                lighter.EncircleRectangle(current.Start, current.End, ChangeColoration.CauseOffFour);

                if (i + 1 < _chain.Count)
                {
                    lighter.CreateLink(current.End, _chain[i + 1].Start, LinkStrength.Weak);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}