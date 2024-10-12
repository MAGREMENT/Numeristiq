using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference;

public class AlternatingInferenceGeneralization<T> : SudokuStrategy, ICommitComparer<NumericChange> where T : ISudokuElement
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

        var rankDiff = r2!.MaxRank() - r1!.MaxRank();
        return rankDiff == 0 ? r2.Length() - r1.Length() : rankDiff;
    }
}

public interface IAlternatingInferenceType<T> where T : ISudokuElement
{
    public string LoopName { get; }
    public string ChainName { get; }
    public Difficulty Difficulty { get; }
    SudokuStrategy? Strategy { set; get; }
    
    IGraph<T, LinkStrength> GetGraph(ISudokuSolverData solverData);

    bool ProcessFullLoop(ISudokuSolverData solverData, Loop<T, LinkStrength> loop);

    bool ProcessWeakInferenceLoop(ISudokuSolverData solverData, T inference, Loop<T, LinkStrength> loop);

    bool ProcessStrongInferenceLoop(ISudokuSolverData solverData, T inference, Loop<T, LinkStrength> loop);

    bool ProcessChain(ISudokuSolverData solverData, Chain<T, LinkStrength> chain, IGraph<T, LinkStrength> graph);

    static bool ProcessChainWithSimpleGraph(ISudokuSolverData solverData, Chain<CellPossibility, LinkStrength> chain,
        IGraph<CellPossibility, LinkStrength> graph, SudokuStrategy strategy)
    {
        if (chain.Elements.Count < 3 || chain.Elements.Count % 2 == 1) return false;

        foreach (var target in graph.Neighbors(chain.Elements[0]))
        {
            if (graph.AreNeighbors(target, chain.Elements[^1]))
                solverData.ChangeBuffer.ProposePossibilityRemoval(target);
        }

        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceChainReportBuilder<CellPossibility>(chain));
        return strategy.StopOnFirstCommit;
    }
    
    static bool ProcessChainWithComplexGraph(ISudokuSolverData solverData, Chain<ISudokuElement, LinkStrength> chain,
        IGraph<ISudokuElement, LinkStrength> graph, SudokuStrategy strategy)
    {
        if (chain.Elements.Count < 3 || chain.Elements.Count % 2 == 1) return false;

        foreach (var target in graph.Neighbors(chain.Elements[0]))
        {
            if (target is not CellPossibility cp) continue;
            
            if (graph.AreNeighbors(target, chain.Elements[^1]))
                solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
        }

        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceChainReportBuilder<ISudokuElement>(chain));
        return strategy.StopOnFirstCommit;
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

public class AlternatingInferenceLoopReportBuilder<T> : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>, IReportBuilderWithChain where T : ISudokuElement
{
    private readonly Loop<T, LinkStrength> _loop;
    private readonly LoopType _type;

    public AlternatingInferenceLoopReportBuilder(Loop<T, LinkStrength> loop, LoopType type)
    {
        _loop = loop;
        _type = type;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Explanation(),
            lighter =>
            {
                var coloring = _loop.Links[0] == LinkStrength.Strong
                    ? StepColor.Cause1
                    : StepColor.On;
                
                foreach (var element in _loop)
                {
                    lighter.HighlightElement(element, coloring);
                    coloring = coloring == StepColor.On
                        ? StepColor.Cause1
                        : StepColor.On;
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
        return _loop.Elements.Count;
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public enum LoopType
{
    NiceLoop, WeakInference, StrongInference
}

public class AlternatingInferenceChainReportBuilder<T> : IChangeReportBuilder<NumericChange, ISudokuSolvingState,
    ISudokuHighlighter>, IReportBuilderWithChain where T : ISudokuElement
{
    private readonly Chain<T, LinkStrength> _chain;

    public AlternatingInferenceChainReportBuilder(Chain<T, LinkStrength> chain)
    {
        _chain = chain;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Explanation(),
            lighter =>
            {
                var coloring = _chain.Links[0] == LinkStrength.Strong
                    ? StepColor.Cause1
                    : StepColor.On;
                
                foreach (var element in _chain)
                {
                    lighter.HighlightElement(element, coloring);
                    coloring = coloring == StepColor.On
                        ? StepColor.Cause1
                        : StepColor.On;
                }

                for (int i = 0; i < _chain.Links.Count; i++)
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
        return _chain.Elements.Count;
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

