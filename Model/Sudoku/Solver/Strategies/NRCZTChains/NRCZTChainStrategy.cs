using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Sudoku.Solver.Strategies.NRCZTChains;

public class NRCZTChainStrategy : SudokuStrategy, ICustomCommitComparer<IUpdatableSudokuSolvingState>
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

    public override void Apply(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink,
            ConstructRule.CellStrongLink, ConstructRule.CellWeakLink);
        var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;

        foreach (var start in graph)
        {
            HashSet<CellPossibility> startVisited = new();
            HashSet<CellPossibility> endVisited = new();

            startVisited.Add(start);
            
            foreach (var friend in graph.Neighbors(start, LinkStrength.Strong))
            {
                if (start == friend || endVisited.Contains(friend)) continue;

                endVisited.Add(friend);
                if (Search(strategyUser, graph, startVisited, endVisited,
                        new BlockChain(new Block(start, friend), graph))) return;
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, ILinkGraph<CellPossibility> graph,
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
                    if (Check(strategyUser, chain, graph)) return true;
                    if (Search(strategyUser, graph, startVisited, endVisited, chain)) return true; 
                }
                
                chain.RemoveLast(graph);
            }

            foreach (var condition in _conditions)
            {
                foreach (var (bEnd, manipulation) in condition.SearchEndUnderCondition(strategyUser,
                             graph, chain, bStart))
                {
                    if (bStart == bEnd || bEnd == chain[0].Start || all.Contains(bEnd) || endVisited.Contains(bEnd)) continue;
                    
                    var block = new Block(bStart, bEnd);
                    chain.Add(block);
                    endVisited.Add(bEnd);

                    manipulation.BeforeSearch(chain, graph);

                    if (chain.PossibleTargets.Count > 0)
                    {
                        if (Check(strategyUser, chain, graph)) return true;
                        if (Search(strategyUser, graph, startVisited, endVisited, chain)) return true; 
                    }
                    
                    manipulation.AfterSearch(chain, graph);
                
                    chain.RemoveLast(graph);
                }
            }
        }

        return false;
    }

    private bool Check(IStrategyUser strategyUser, BlockChain chain, ILinkGraph<CellPossibility> graph)
    {
        var last = chain.Last().End;
        
        foreach (var target in chain.PossibleTargets)
        {
            if (graph.AreNeighbors(target, last)) strategyUser.ChangeBuffer.ProposePossibilityRemoval(target);
        }

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
            new NRCChainReportBuilder(chain.Copy())) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    public int Compare(ChangeCommit<IUpdatableSudokuSolvingState> first, ChangeCommit<IUpdatableSudokuSolvingState> second)
    {
        if (first.Builder is not NRCChainReportBuilder r1 ||
            second.Builder is not NRCChainReportBuilder r2) return 0;

        return r2.Chain.Count - r1.Chain.Count;
    }
}

public class NRCChainReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState>
{
    public BlockChain Chain { get; }

    public NRCChainReportBuilder(BlockChain chain)
    {
        Chain = chain;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport( Explanation(), lighter =>
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

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        return Chain.ToString();
    }
}