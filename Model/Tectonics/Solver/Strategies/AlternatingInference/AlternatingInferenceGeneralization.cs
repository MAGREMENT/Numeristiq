using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
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

    private bool Search(ITectonicSolverData solverData, ILinkGraph<ITectonicElement> graph, CellPossibility start)
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
                if (!on.TryAdd(eOn, current)) continue;
                
                /*if (((off.TryGetValue(eOn, out var before) && !before.Equals(current)) 
                    || (eOn is CellPossibility cp1 && cp1 == start))
                    && TryFindLoop(current, eOn, off, on) >= 4)
                {
                    if(TryProcessLoop(solverData, eOn, true, on, off)) return true;
                    continue;
                }*/ //TODO fix this dumb shit

                if (TryProcessChain(solverData, start, eOn, on, off)) return true;

                foreach (var eOff in graph.Neighbors(eOn))
                {
                    if (eOff is CellPossibility cp2 && cp2 == start) continue;
                    
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

    private bool TryProcessLoop(ITectonicSolverData solverData, ITectonicElement current, bool isStrong,
        Dictionary<ITectonicElement, ITectonicElement> on, Dictionary<ITectonicElement, ITectonicElement> off)
    {
        if (current is not CellPossibility cp) return false;
        if (isStrong) solverData.ChangeBuffer.ProposeSolutionAddition(cp);
        else solverData.ChangeBuffer.ProposePossibilityRemoval(cp);
        
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder(current, isStrong, on, off));
        return StopOnFirstCommit;
    }

    private int TryFindLoop(ITectonicElement current, ITectonicElement objective, Dictionary<ITectonicElement, ITectonicElement> start,
        Dictionary<ITectonicElement, ITectonicElement> other)
    {
        var result = 1;
        bool s = true;
        while ((s ? start : other).TryGetValue(current, out var next))
        {
            if (next.Equals(objective)) return result;
            
            result++;
            current = next;
            s = !s;
        }

        return -1;
    }
}

public interface IAlternatingInferenceType
{
    string Name { get; }
    
    Difficulty Difficulty { get; }

    ILinkGraph<ITectonicElement> GetGraph(ITectonicSolverData solverData);
}

public static class AlternatingInferenceReportBuilderHelper
{
    public static Chain<ITectonicElement, LinkStrength> ReconstructChain(Dictionary<ITectonicElement, ITectonicElement> on,
        Dictionary<ITectonicElement, ITectonicElement> off, ITectonicElement startElement, ITectonicElement? endElement,
        LinkStrength firstLink, bool isLoop)
    {
        List<ITectonicElement> e = new();
        List<LinkStrength> l = new();

        e.Add(startElement);
        var link = firstLink;
        var current = on;
        while (current.TryGetValue(e[^1], out var friend))
        {
            if (friend.Equals(endElement)) break;
            
            e.Add(friend);
            l.Add(link);
            if (link == firstLink)
            {
                link = firstLink == LinkStrength.Strong ? LinkStrength.Weak : LinkStrength.Strong;
                current = off;
            }
            else
            {
                link = firstLink;
                current = on;
            }
        }
        
        e.Reverse();
        l.Reverse();

        return isLoop ? new Loop<ITectonicElement, LinkStrength>(e.ToArray(), l.ToArray(), firstLink) :
            new Chain<ITectonicElement, LinkStrength>(e.ToArray(), l.ToArray());
    }

    public static string Description(Chain<ITectonicElement, LinkStrength> chain)
    {
        var builder = new StringBuilder();
        var shift = 0;
        if (chain is Loop<ITectonicElement, LinkStrength>)
        {
            shift++;
            builder.Append("Loop : ");
        }
        else builder.Append("Chain : ");
        
        builder.Append(chain.Elements[0]);

        for (int i = 0; i < chain.Links.Length - shift; i++)
        {
            var link = chain.Links[i] == LinkStrength.Strong ? '=' : '-';
            builder.Append($" {link} {chain.Elements[i + 1]}");
        }

        if (shift > 0) builder.Append($" {chain.Links[^1]}");

        return builder.ToString();
    }
}

public class AlternatingInferenceLoopReportBuilder : IChangeReportBuilder<NumericChange, INumericSolvingState,
        ITectonicHighlighter>
{
    private readonly ITectonicElement _loopPoint;
    private readonly bool _isStrong;
    private readonly Dictionary<ITectonicElement, ITectonicElement> _on;
    private readonly Dictionary<ITectonicElement, ITectonicElement> _off;

    public AlternatingInferenceLoopReportBuilder(ITectonicElement loopPoint, bool isStrong, Dictionary<ITectonicElement, ITectonicElement> on, Dictionary<ITectonicElement, ITectonicElement> off)
    {
        _loopPoint = loopPoint;
        _isStrong = isStrong;
        _on = on;
        _off = off;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        var loop = (Loop<ITectonicElement, LinkStrength>)(_isStrong
            ? AlternatingInferenceReportBuilderHelper.ReconstructChain(_off, _on, _loopPoint,
                _loopPoint, LinkStrength.Strong, true)
            : AlternatingInferenceReportBuilderHelper.ReconstructChain(_on, _off, _loopPoint,
                _loopPoint, LinkStrength.Weak, true));
        return new ChangeReport<ITectonicHighlighter>(AlternatingInferenceReportBuilderHelper.Description(loop),
            lighter =>
            {
                lighter.HighlightElement(loop.Elements[0], StepColor.Cause1);
            
                for (int i = 0; i < loop.Links.Length - 1; i++)
                {
                    lighter.CreateLink(loop.Elements[i], loop.Elements[i + 1], loop.Links[i]);
                    lighter.HighlightElement(loop.Elements[i + 1], loop.Links[i] == LinkStrength.Strong ? StepColor.On :
                        StepColor.Cause1);
                }

                lighter.CreateLink(loop.Elements[^1], loop.Elements[0], loop.Links[^1]);

                ChangeReportHelper.HighlightChanges(lighter, changes);
            });
    }

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
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
        var chain = AlternatingInferenceReportBuilderHelper.ReconstructChain(_on, _off, _end, null,
            LinkStrength.Strong, false);
        return new ChangeReport<ITectonicHighlighter>(AlternatingInferenceReportBuilderHelper.Description(chain),
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