using System.Collections.Generic;
using System.Text;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Tectonics.Utility;
using Model.Utility;

namespace Model.Tectonics.Strategies.AlternatingInference;

public class AlternatingInferenceGeneralization : TectonicStrategy //TODO manage loops
{
    private readonly IAlternatingInferenceType _type;
    
    public AlternatingInferenceGeneralization(IAlternatingInferenceType type) 
        : base(type.Name, type.Difficulty, InstanceHandling.BestOnly)
    {
        _type = type;
    }

    public override void Apply(ITectonicStrategyUser strategyUser)
    {
        var graph = _type.GetGraph(strategyUser);
        foreach (var element in graph)
        {
            if (element is not CellPossibility cp) continue;
            if (Search(strategyUser, graph, cp)) return;
        }
    }

    private bool Search(ITectonicStrategyUser strategyUser, ILinkGraph<ITectonicElement> graph, CellPossibility start)
    {
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

                if (TryProcess(strategyUser, start, eOn, on, off)) return true;

                foreach (var eOff in graph.Neighbors(eOn))
                {
                    if (eOff is CellPossibility cp && cp == start) continue;
                    
                    if (off.TryAdd(eOff, eOn)) queue.Enqueue(eOff);
                }
            }
        }

        return false;
    }

    private bool TryProcess(ITectonicStrategyUser strategyUser, CellPossibility start, ITectonicElement current,
        Dictionary<ITectonicElement, ITectonicElement> on, Dictionary<ITectonicElement, ITectonicElement> off)
    {
        if (current is not CellPossibility end) return false;

        foreach (var cp in TectonicCellUtility.SharedSeenPossibilities(strategyUser, start, end))
        {
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(cp);
        }

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
            new AlternatingInferenceReportBuilder(end, on, off)) && StopOnFirstPush;
    }
}

public interface IAlternatingInferenceType
{
    string Name { get; }
    
    StepDifficulty Difficulty { get; }

    ILinkGraph<ITectonicElement> GetGraph(ITectonicStrategyUser strategyUser);
}

public class AlternatingInferenceReportBuilder : IChangeReportBuilder<ITectonicSolvingState, ITectonicHighlighter>
{
    private readonly CellPossibility _end;
    private readonly Dictionary<ITectonicElement, ITectonicElement> _on;
    private readonly Dictionary<ITectonicElement, ITectonicElement> _off;

    public AlternatingInferenceReportBuilder(CellPossibility end, 
        Dictionary<ITectonicElement, ITectonicElement> on, Dictionary<ITectonicElement, ITectonicElement> off)
    {
        _end = end;
        _on = on;
        _off = off;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
    {
        var chain = ReconstructChain();
        return new ChangeReport<ITectonicHighlighter>(Description(chain), lighter =>
        {
            lighter.HighlightElement(chain.Elements[0], ChangeColoration.CauseOffOne);
            
            for (int i = 0; i < chain.Links.Length; i++)
            {
                lighter.CreateLink(chain.Elements[i], chain.Elements[i + 1], chain.Links[i]);
                lighter.HighlightElement(chain.Elements[i + 1], chain.Links[i] == LinkStrength.Strong ? ChangeColoration.CauseOnOne :
                    ChangeColoration.CauseOffOne);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description(Chain<ITectonicElement, LinkStrength> chain)
    {
        var builder = new StringBuilder(chain.Elements[0].ToString());

        for (int i = 0; i < chain.Links.Length; i++)
        {
            var link = chain.Links[i] == LinkStrength.Strong ? '=' : '-';
            builder.Append($" {link} {chain.Elements[i + 1]}");
        }

        return builder.ToString();
    }

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }

    private Chain<ITectonicElement, LinkStrength> ReconstructChain()
    {
        List<ITectonicElement> e = new();
        List<LinkStrength> l = new();

        e.Add(_end);
        var link = LinkStrength.Strong;
        var current = _on;
        while (current.TryGetValue(e[^1], out var friend))
        {
            e.Add(friend);
            l.Add(link);
            if (link == LinkStrength.Strong)
            {
                link = LinkStrength.Weak;
                current = _off;
            }
            else
            {
                link = LinkStrength.Strong;
                current = _on;
            }
        }
        
        e.Reverse();
        l.Reverse();

        return new Chain<ITectonicElement, LinkStrength>(e.ToArray(), l.ToArray());
    }
}