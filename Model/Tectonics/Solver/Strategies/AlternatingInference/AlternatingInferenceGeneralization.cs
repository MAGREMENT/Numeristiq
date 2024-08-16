using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Solver.Utility;
using Model.Utility;

namespace Model.Tectonics.Solver.Strategies.AlternatingInference;

public class AlternatingInferenceGeneralization : Strategy<ITectonicSolverData>
{
    private readonly IAlternatingInferenceType _type;
    
    public AlternatingInferenceGeneralization(IAlternatingInferenceType type) 
        : base(type.Name, type.Difficulty, InstanceHandling.BestOnly)
    {
        _type = type;
    }

    public override void Apply(ITectonicSolverData data)
    {
        var graph = _type.GetGraph(data);
        foreach (var element in graph)
        {
            if (element is not CellPossibility cp) continue;
            if (Search(data, graph, cp)) return;
        }
    }

    private bool Search(ITectonicSolverData solverData, IGraph<ITectonicElement, LinkStrength> graph, CellPossibility start)
    {
        //TODO multiple calls for same cell ???
        
        Dictionary<ITectonicElement, ITectonicElement> on = new();
        Dictionary<ITectonicElement, ITectonicElement> off = new();
        Queue<ITectonicElement> queue = new();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var eOn in graph.Neighbors(current, LinkStrength.Strong))
            {
                if (IsInPath(current, eOn, off, on) || !on.TryAdd(eOn, current)) continue;

                if (TryProcessChain(solverData, start, eOn, on, off)) return true;

                foreach (var eOff in graph.Neighbors(eOn))
                {
                    if (eOff is CellPossibility cp2 && cp2 == start || IsInPath(eOn, eOff, on, off))
                        continue;
                    
                    if (off.TryAdd(eOff, eOn)) queue.Enqueue(eOff);
                }
            }
        }

        return false;
    }

    private bool TryProcessChain(ITectonicSolverData solverData, CellPossibility start, ITectonicElement current,
        Dictionary<ITectonicElement, ITectonicElement> on, Dictionary<ITectonicElement, ITectonicElement> off)
    {
        if (current is not CellPossibility end) return false;

        foreach (var cp in TectonicUtility.SharedSeenPossibilities(solverData, start, end))
        {
            solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
        }

        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceChainReportBuilder(end, on, off));
        return StopOnFirstCommit;
    }

    private static bool IsInPath(ITectonicElement from, ITectonicElement target,
        Dictionary<ITectonicElement, ITectonicElement> first, Dictionary<ITectonicElement, ITectonicElement> second)
    {
        var isFirst = true;
        var current = from;

        while (isFirst ? first.TryGetValue(current, out var next) : second.TryGetValue(current, out next))
        {
            if (next.Equals(target)) return true;

            current = next;
            isFirst = !isFirst;
        }

        return false;
    }
}

public interface IAlternatingInferenceType
{
    string Name { get; }
    
    Difficulty Difficulty { get; }

    IGraph<ITectonicElement, LinkStrength> GetGraph(ITectonicSolverData solverData);
}

public class AlternatingInferenceChainReportBuilder : IChangeReportBuilder<NumericChange, INumericSolvingState, ITectonicHighlighter>
{
    private readonly CellPossibility _end;
    private readonly Dictionary<ITectonicElement, ITectonicElement> _on;
    private readonly Dictionary<ITectonicElement, ITectonicElement> _off;

    public AlternatingInferenceChainReportBuilder(CellPossibility end, 
        Dictionary<ITectonicElement, ITectonicElement> on, Dictionary<ITectonicElement, ITectonicElement> off)
    {
        _end = end;
        _on = on;
        _off = off;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        var chain = ChainExtensions.ReconstructChain(_on, _off, _end, LinkStrength.Strong, false);
        return new ChangeReport<ITectonicHighlighter>($"Chain : {chain.ToLinkChainString()}",
            lighter =>
        {
            lighter.HighlightElement(chain.Elements[0], StepColor.Cause1);
            
            for (int i = 0; i < chain.Links.Length; i++)
            {
                lighter.CreateLink(chain.Elements[i], chain.Elements[i + 1], chain.Links[i]);
                lighter.HighlightElement(chain.Elements[i + 1], chain.Links[i] == LinkStrength.Strong ? StepColor.On :
                    StepColor.Cause1);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}