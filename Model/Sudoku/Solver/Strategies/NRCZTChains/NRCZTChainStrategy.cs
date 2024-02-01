using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Sudoku.Solver.Strategies.NRCZTChains;

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

        foreach (var start in graph)
        {
            HashSet<CellPossibility> startVisited = new();
            HashSet<CellPossibility> endVisited = new();

            startVisited.Add(start);
            
            foreach (var friend in graph.Neighbors(start, LinkStrength.Strong))
            {
                if (start == friend || endVisited.Contains(friend)) continue;

                endVisited.Add(friend);
                if (Search(strategyManager, graph, startVisited, endVisited,
                        new BlockChain(new Block(start, friend), graph))) return;
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, ILinkGraph<CellPossibility> graph,
        HashSet<CellPossibility> startVisited, HashSet<CellPossibility> endVisited, BlockChain chain)
    {
        var all = chain.AllCellPossibilities();

        foreach (var bStart in graph.Neighbors(chain.Last().End))
        {
            if (all.Contains(bStart) || startVisited.Contains(bStart)) continue;

            startVisited.Add(bStart);

            foreach (var bEnd in graph.Neighbors(bStart, LinkStrength.Strong))
            {
                if (bStart == bEnd || bEnd == chain[0].Start || all.Contains(bEnd) || endVisited.Contains(bEnd)) continue;
                
                var block = new Block(bStart, bEnd);
                chain.Add(block);
                endVisited.Add(bEnd);

                if (chain.PossibleTargets.Count > 0)
                {
                    if (Check(strategyManager, chain, graph)) return true;
                    if (Search(strategyManager, graph, startVisited, endVisited, chain)) return true; 
                }
                
                chain.RemoveLast(graph);
            }

            foreach (var condition in _conditions)
            {
                foreach (var (bEnd, manipulation) in condition.SearchEndUnderCondition(strategyManager,
                             graph, chain, bStart))
                {
                    if (bStart == bEnd || bEnd == chain[0].Start || all.Contains(bEnd) || endVisited.Contains(bEnd)) continue;
                    
                    var block = new Block(bStart, bEnd);
                    chain.Add(block);
                    endVisited.Add(bEnd);

                    manipulation.BeforeSearch(chain, graph);

                    if (chain.PossibleTargets.Count > 0)
                    {
                        if (Check(strategyManager, chain, graph)) return true;
                        if (Search(strategyManager, graph, startVisited, endVisited, chain)) return true; 
                    }
                    
                    manipulation.AfterSearch(chain, graph);
                
                    chain.RemoveLast(graph);
                }
            }
        }

        return false;
    }

    private bool Check(IStrategyManager strategyManager, BlockChain chain, ILinkGraph<CellPossibility> graph)
    {
        var last = chain.Last().End;
        
        foreach (var target in chain.PossibleTargets)
        {
            if (graph.AreNeighbors(target, last)) strategyManager.ChangeBuffer.ProposePossibilityRemoval(target);
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
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