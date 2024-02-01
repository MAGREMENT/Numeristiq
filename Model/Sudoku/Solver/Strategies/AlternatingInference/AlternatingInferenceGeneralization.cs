using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies.AlternatingInference;

public class AlternatingInferenceGeneralization<T> : AbstractStrategy, ICustomCommitComparer where T : ISudokuElement
{
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.ChooseBest;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    private readonly IAlternatingInferenceType<T> _type;
    private readonly IAlternatingInferenceAlgorithm<T> _algorithm;

    public AlternatingInferenceGeneralization(IAlternatingInferenceType<T> type, IAlternatingInferenceAlgorithm<T> algo)
        : base("", type.Difficulty, DefaultBehavior)
    {
        Name = algo.Type == AlgorithmType.Loop ? type.LoopName : type.ChainName;
        _type = type;
        _type.Strategy = this;
        _algorithm = algo;
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        _algorithm.Run(strategyManager, _type);
    }

    public int Compare(ChangeCommit first, ChangeCommit second)
    {
        if (first.Builder is not IReportBuilderWithChain r1 ||
            second.Builder is not IReportBuilderWithChain r2) return 0;

        var rankDiff = r2.MaxRank() - r1.MaxRank();
        return rankDiff == 0 ? r2.Length() - r1.Length() : rankDiff;
    }
}

public interface IAlternatingInferenceType<T> where T : ISudokuElement
{
    public string LoopName { get; }
    public string ChainName { get; }
    public StrategyDifficulty Difficulty { get; }
    IStrategy? Strategy { set; get; }
    
    ILinkGraph<T> GetGraph(IStrategyManager strategyManager);

    bool ProcessFullLoop(IStrategyManager strategyManager, LinkGraphLoop<T> loop);

    bool ProcessWeakInferenceLoop(IStrategyManager strategyManager, T inference, LinkGraphLoop<T> loop);

    bool ProcessStrongInferenceLoop(IStrategyManager strategyManager, T inference, LinkGraphLoop<T> loop);

    bool ProcessChain(IStrategyManager strategyManager, LinkGraphChain<T> chain, ILinkGraph<T> graph);

    static bool ProcessChainWithSimpleGraph(IStrategyManager strategyManager, LinkGraphChain<CellPossibility> chain,
        ILinkGraph<CellPossibility> graph, IStrategy strategy)
    {
        if (chain.Count < 3 || chain.Count % 2 == 1) return false;

        foreach (var target in graph.Neighbors(chain.Elements[0]))
        {
            if (graph.AreNeighbors(target, chain.Elements[^1]))
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(target);
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(strategy,
                   new AlternatingInferenceChainReportBuilder<CellPossibility>(chain)) &&
                            strategy.OnCommitBehavior == OnCommitBehavior.Return;
    }
    
    static bool ProcessChainWithComplexGraph(IStrategyManager strategyManager, LinkGraphChain<ISudokuElement> chain,
        ILinkGraph<ISudokuElement> graph, IStrategy strategy)
    {
        if (chain.Count < 3 || chain.Count % 2 == 1) return false;

        foreach (var target in graph.Neighbors(chain.Elements[0]))
        {
            if (target is not CellPossibility cp) continue;
            
            if (graph.AreNeighbors(target, chain.Elements[^1]))
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(cp);
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(strategy,
                   new AlternatingInferenceChainReportBuilder<ISudokuElement>(chain)) &&
                            strategy.OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public interface IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    AlgorithmType Type { get; }
    void Run(IStrategyManager strategyManager, IAlternatingInferenceType<T> type);
}

public enum AlgorithmType
{
    Loop, Chain
}

public interface IReportBuilderWithChain
{
    public int MaxRank();
    public int Length();
}

public class AlternatingInferenceLoopReportBuilder<T> : IChangeReportBuilder, IReportBuilderWithChain where T : ISudokuElement
{
    private readonly LinkGraphLoop<T> _loop;
    private readonly LoopType _type;

    public AlternatingInferenceLoopReportBuilder(LinkGraphLoop<T> loop, LoopType type)
    {
        _loop = loop;
        _type = type;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(),
            lighter =>
            {
                var coloring = _loop.Links[0] == LinkStrength.Strong
                    ? ChangeColoration.CauseOffOne
                    : ChangeColoration.CauseOnOne;
                
                foreach (var element in _loop)
                {
                    lighter.HighlightLinkGraphElement(element, coloring);
                    coloring = coloring == ChangeColoration.CauseOnOne
                        ? ChangeColoration.CauseOffOne
                        : ChangeColoration.CauseOnOne;
                }
                
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }

    private string Explanation()
    {
        var result = _type switch
        {
            LoopType.NiceLoop => "Nice loop",
            LoopType.StrongInference => "Loop with a strong inference",
            LoopType.WeakInference => "Loop with a weak inference",
            _ => throw new ArgumentOutOfRangeException()
        };

        return result + $" found\nLoop :: {_loop}";
    }

    public int MaxRank()
    {
        return _loop.MaxRank();
    }

    public int Length()
    {
        return _loop.Count;
    }
}

public enum LoopType
{
    NiceLoop, WeakInference, StrongInference
}

public class AlternatingInferenceChainReportBuilder<T> : IChangeReportBuilder, IReportBuilderWithChain where T : ISudokuElement
{
    private readonly LinkGraphChain<T> _chain;

    public AlternatingInferenceChainReportBuilder(LinkGraphChain<T> chain)
    {
        _chain = chain;
    }

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(),
            lighter =>
            {
                var coloring = _chain.Links[0] == LinkStrength.Strong
                    ? ChangeColoration.CauseOffOne
                    : ChangeColoration.CauseOnOne;
                
                foreach (var element in _chain)
                {
                    lighter.HighlightLinkGraphElement(element, coloring);
                    coloring = coloring == ChangeColoration.CauseOnOne
                        ? ChangeColoration.CauseOffOne
                        : ChangeColoration.CauseOnOne;
                }

                for (int i = 0; i < _chain.Links.Length; i++)
                {
                    lighter.CreateLink(_chain.Elements[i], _chain.Elements[i + 1], _chain.Links[i]);
                }
                
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }

    private string Explanation()
    {
        return "";
    }

    public int MaxRank()
    {
        return _chain.MaxRank();
    }

    public int Length()
    {
        return _chain.Count;
    }
}

