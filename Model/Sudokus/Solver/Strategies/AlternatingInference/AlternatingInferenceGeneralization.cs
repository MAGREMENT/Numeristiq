using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference;

public class AlternatingInferenceGeneralization<T> : SudokuStrategy, ICommitComparer where T : ISudokuElement
{
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.BestOnly;

    private readonly IAlternatingInferenceType<T> _type;
    private readonly IAlternatingInferenceAlgorithm<T> _algorithm;

    public AlternatingInferenceGeneralization(IAlternatingInferenceType<T> type, IAlternatingInferenceAlgorithm<T> algo)
        : base("", type.Difficulty, DefaultInstanceHandling)
    {
        Name = algo.Type == AlgorithmType.Loop ? type.LoopName : type.ChainName;
        _type = type;
        _type.Strategy = this;
        _algorithm = algo;
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        _algorithm.Run(solverData, _type);
    }

    public int Compare(IChangeCommit<NumericChange> first, IChangeCommit<NumericChange> second)
    {
        if (first.TryGetBuilder<IReportBuilderWithChain>(out var r1) ||
            second.TryGetBuilder<IReportBuilderWithChain>(out var r2)) return 0;

        var rankDiff = r2.MaxRank() - r1.MaxRank();
        return rankDiff == 0 ? r2.Length() - r1.Length() : rankDiff;
    }
}

public interface IAlternatingInferenceType<T> where T : ISudokuElement
{
    public string LoopName { get; }
    public string ChainName { get; }
    public StepDifficulty Difficulty { get; }
    SudokuStrategy? Strategy { set; get; }
    
    ILinkGraph<T> GetGraph(ISudokuSolverData solverData);

    bool ProcessFullLoop(ISudokuSolverData solverData, LinkGraphLoop<T> loop);

    bool ProcessWeakInferenceLoop(ISudokuSolverData solverData, T inference, LinkGraphLoop<T> loop);

    bool ProcessStrongInferenceLoop(ISudokuSolverData solverData, T inference, LinkGraphLoop<T> loop);

    bool ProcessChain(ISudokuSolverData solverData, LinkGraphChain<T> chain, ILinkGraph<T> graph);

    static bool ProcessChainWithSimpleGraph(ISudokuSolverData solverData, LinkGraphChain<CellPossibility> chain,
        ILinkGraph<CellPossibility> graph, SudokuStrategy strategy)
    {
        if (chain.Count < 3 || chain.Count % 2 == 1) return false;

        foreach (var target in graph.Neighbors(chain.Elements[0]))
        {
            if (graph.AreNeighbors(target, chain.Elements[^1]))
                solverData.ChangeBuffer.ProposePossibilityRemoval(target);
        }

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                   new AlternatingInferenceChainReportBuilder<CellPossibility>(chain)) &&
                            strategy.StopOnFirstPush;
    }
    
    static bool ProcessChainWithComplexGraph(ISudokuSolverData solverData, LinkGraphChain<ISudokuElement> chain,
        ILinkGraph<ISudokuElement> graph, SudokuStrategy strategy)
    {
        if (chain.Count < 3 || chain.Count % 2 == 1) return false;

        foreach (var target in graph.Neighbors(chain.Elements[0]))
        {
            if (target is not CellPossibility cp) continue;
            
            if (graph.AreNeighbors(target, chain.Elements[^1]))
                solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
        }

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                   new AlternatingInferenceChainReportBuilder<ISudokuElement>(chain)) &&
                            strategy.StopOnFirstPush;
    }
}

public interface IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    AlgorithmType Type { get; }
    void Run(ISudokuSolverData solverData, IAlternatingInferenceType<T> type);
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

public class AlternatingInferenceLoopReportBuilder<T> : IChangeReportBuilder<NumericChange, IUpdatableSudokuSolvingState, ISudokuHighlighter>, IReportBuilderWithChain where T : ISudokuElement
{
    private readonly LinkGraphLoop<T> _loop;
    private readonly LoopType _type;

    public AlternatingInferenceLoopReportBuilder(LinkGraphLoop<T> loop, LoopType type)
    {
        _loop = loop;
        _type = type;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Explanation(),
            lighter =>
            {
                var coloring = _loop.Links[0] == LinkStrength.Strong
                    ? ChangeColoration.CauseOffOne
                    : ChangeColoration.CauseOnOne;
                
                foreach (var element in _loop)
                {
                    lighter.HighlightElement(element, coloring);
                    coloring = coloring == ChangeColoration.CauseOnOne
                        ? ChangeColoration.CauseOffOne
                        : ChangeColoration.CauseOnOne;
                }
                
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Strong), LinkStrength.Strong);
                _loop.ForEachLink((one, two) => lighter.CreateLink(one, two, LinkStrength.Weak), LinkStrength.Weak);
                
                ChangeReportHelper.HighlightChanges(lighter, changes);
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
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public enum LoopType
{
    NiceLoop, WeakInference, StrongInference
}

public class AlternatingInferenceChainReportBuilder<T> : IChangeReportBuilder<NumericChange, IUpdatableSudokuSolvingState, ISudokuHighlighter>, IReportBuilderWithChain where T : ISudokuElement
{
    private readonly LinkGraphChain<T> _chain;

    public AlternatingInferenceChainReportBuilder(LinkGraphChain<T> chain)
    {
        _chain = chain;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Explanation(),
            lighter =>
            {
                var coloring = _chain.Links[0] == LinkStrength.Strong
                    ? ChangeColoration.CauseOffOne
                    : ChangeColoration.CauseOnOne;
                
                foreach (var element in _chain)
                {
                    lighter.HighlightElement(element, coloring);
                    coloring = coloring == ChangeColoration.CauseOnOne
                        ? ChangeColoration.CauseOffOne
                        : ChangeColoration.CauseOnOne;
                }

                for (int i = 0; i < _chain.Links.Length; i++)
                {
                    lighter.CreateLink(_chain.Elements[i], _chain.Elements[i + 1], _chain.Links[i]);
                }
                
                ChangeReportHelper.HighlightChanges(lighter, changes);
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
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

