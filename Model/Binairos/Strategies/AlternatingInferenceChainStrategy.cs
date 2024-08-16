using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Binairos.Strategies;

public class AlternatingInferenceChainStrategy : Strategy<IBinairoSolverData> //TODO error chain
{
    public AlternatingInferenceChainStrategy() : base("Alternating Inference Chain", Difficulty.Extreme, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(IBinairoSolverData data)
    {
        data.ManagedGraph.Construct(BinairoConstructRule.Instance);
        var graph = data.ManagedGraph.Graph;
        foreach (var element in graph)
        {
            if (Search(data, graph, element)) return;
        }
    }

    private bool Search(IBinairoSolverData solverData, IGraph<CellPossibility, LinkStrength> graph, CellPossibility start)
    {
        Dictionary<CellPossibility, CellPossibility> on = new();
        Dictionary<CellPossibility, CellPossibility> off = new();
        Queue<CellPossibility> queue = new();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var eOn in graph.Neighbors(current, LinkStrength.Strong))
            {
                if (IsInPath(current, eOn, off, on) || !on.TryAdd(eOn, current)) continue;

                if (TryProcessChain(solverData, start, eOn, on, off, graph)) return true;

                foreach (var eOff in graph.Neighbors(eOn))
                {
                    if (eOff == start || IsInPath(eOn, eOff, on, off))
                        continue;
                    
                    if (off.TryAdd(eOff, eOn)) queue.Enqueue(eOff);
                }
            }
        }

        return false;
    }

    private bool TryProcessChain(IBinairoSolverData solverData, CellPossibility start, CellPossibility current,
        Dictionary<CellPossibility, CellPossibility> on, Dictionary<CellPossibility, CellPossibility> off,
        IGraph<CellPossibility, LinkStrength> graph)
    {
        if (current.Possibility == start.Possibility) return false;
        
        foreach (var target in graph.Neighbors(current))
        {
            if(graph.AreNeighbors(new CellPossibility(start.Row, start.Column,
                   BinairoUtility.Opposite(start.Possibility)), target))
                solverData.ChangeBuffer.ProposeSolutionAddition(target);
        }

        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceChainReportBuilder(current, on, off));
        return StopOnFirstCommit;
    }

    private static bool IsInPath(CellPossibility from, CellPossibility target,
        Dictionary<CellPossibility, CellPossibility> first, Dictionary<CellPossibility, CellPossibility> second)
    {
        var isFirst = true;
        var current = from;

        while (isFirst ? first.TryGetValue(current, out var next) : second.TryGetValue(current, out next))
        {
            if (next == target) return true;

            current = next;
            isFirst = !isFirst;
        }

        return false;
    }
}

public class AlternatingInferenceChainReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter>
{
    private readonly CellPossibility _end;
    private readonly Dictionary<CellPossibility, CellPossibility> _on;
    private readonly Dictionary<CellPossibility, CellPossibility> _off;

    public AlternatingInferenceChainReportBuilder(CellPossibility end, 
        Dictionary<CellPossibility, CellPossibility> on, Dictionary<CellPossibility, CellPossibility> off)
    {
        _end = end;
        _on = on;
        _off = off;
    }

    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        var chain = ChainExtensions.ReconstructChain(_on, _off, _end, LinkStrength.Strong, false);
        return new ChangeReport<IBinairoHighlighter>($"Chain : {chain.ToLinkChainString()}",
            lighter =>
            {
                lighter.HighlightCell(chain.Elements[0].ToCell(), StepColor.Cause1);
            
                for (int i = 0; i < chain.Links.Length; i++)
                {
                    //lighter.CreateLink(chain.Elements[i], chain.Elements[i + 1], chain.Links[i]);
                    lighter.HighlightCell(chain.Elements[i + 1].ToCell(), chain.Links[i] == LinkStrength.Strong ? StepColor.On :
                        StepColor.Cause1);
                }

                ChangeReportHelper.HighlightChanges(lighter, changes);
            });
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return Clue<IBinairoHighlighter>.Default();
    }
}