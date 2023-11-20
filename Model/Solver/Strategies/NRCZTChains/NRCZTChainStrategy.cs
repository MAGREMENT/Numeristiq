using System;
using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;
using Model.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Solver.Strategies.NRCZTChains;

public class NRCZTChainStrategy : AbstractStrategy, ICustomCommitComparer
{
    public const string OfficialNameForDefault = "NRC-Chains";
    public const string OfficialNameForTCondition = "NRCT-Chains";
    public const string OfficialNameForZCondition = "NRCZ-Chains";
    public const string OfficialNameForZAndTCondition = "NRCZT-Chains";
    
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.ChooseBest;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly INRCZTCondition[] _conditions;
    
    public NRCZTChainStrategy(params INRCZTCondition[] conditions) : base("", StrategyDifficulty.None, DefaultBehavior)
    {
        _conditions = conditions;

        Difficulty = _conditions.Length > 0 ? StrategyDifficulty.Extreme : StrategyDifficulty.Hard;

        Name = conditions.Length switch
        {
            0 => OfficialNameForDefault,
            1 => $"NRC{conditions[0].Name}-Chains",
            2 => OfficialNameForZAndTCondition,
            _ => throw new ArgumentException("Too many conditions")
        };
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
                if (start == friend || endVisited.Contains(friend)) continue;
                
                var block = new Block(start, friend);
                chain.Add(block);
                endVisited.Add(friend);

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
                if (bStart == bEnd || bEnd == chain[0].Start || endVisited.Contains(bEnd)) continue;
                
                var block = new Block(bStart, bEnd);
                chain.Add(block);
                endVisited.Add(bEnd);

                if (Check(strategyManager, chain, graph)) return true;
                if (Search(strategyManager, graph, startVisited, endVisited, chain)) return true;
                
                chain.RemoveLast();
            }

            foreach (var condition in _conditions)
            {
                foreach (var (bEnd, manipulation) in condition.SearchEndUnderCondition(strategyManager,
                             graph, chain, bStart))
                {
                    if (bStart == bEnd || bEnd == chain[0].Start || endVisited.Contains(bEnd)) continue;
                    
                    var block = new Block(bStart, bEnd);
                    chain.Add(block);
                    endVisited.Add(bEnd);

                    manipulation.BeforeSearch(chain);
                    if (Check(strategyManager, chain, graph)) return true;
                    if (Search(strategyManager, graph, startVisited, endVisited, chain)) return true;
                    manipulation.AfterSearch(chain);
                
                    chain.RemoveLast();
                }
            }
        }

        return false;
    }

    private bool Check(IStrategyManager strategyManager, BlockChain chain, LinkGraph<CellPossibility> graph)
    {
        var first = chain[0].Start;
        var last = chain.Last().End;

        if (chain.MustTarget is not null)
        {
            foreach (var t in chain.MustTarget)
            {
                if (first.AreWeaklyLinked(t) && last.AreWeaklyLinked(t)) 
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(t);
            }

            return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                new NRCChainReportBuilder(chain.Copy())) && OnCommitBehavior == OnCommitBehavior.Return;
        }

        foreach (var weakLink in graph.GetLinks(first))
        {
            if (graph.HasLinkTo(weakLink, last)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(weakLink);
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new NRCChainReportBuilder(chain.Copy())) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    public int Compare(ChangeCommit first, ChangeCommit second)
    {
        if (first.Builder is not NRCChainReportBuilder r1 ||
            second.Builder is not NRCChainReportBuilder r2) return 0;

        return r2.Chain.Count - r1.Chain.Count;
    }
}

public class NRCChainReportBuilder : IChangeReportBuilder
{
    public BlockChain Chain { get; }

    public NRCChainReportBuilder(BlockChain chain)
    {
        Chain = chain;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            for (int i = 0; i < Chain.Count; i++)
            {
                var current = Chain[i];
                
                lighter.HighlightPossibility(current.Start, ChangeColoration.CauseOffOne);
                lighter.HighlightPossibility(current.End, ChangeColoration.CauseOnOne);
                lighter.CreateLink(current.Start, current.End, LinkStrength.Strong);

                if (i + 1 < Chain.Count)
                {
                    lighter.CreateLink(current.End, Chain[i + 1].Start, LinkStrength.Weak);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        return Chain.ToString();
    }
}