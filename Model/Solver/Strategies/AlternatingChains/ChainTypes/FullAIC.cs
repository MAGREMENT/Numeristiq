using System;
using System.Collections.Generic;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class FullAIC : IAlternatingChainType<ILinkGraphElement>
{
    public const string OfficialName = "Alternating Inference Chain";
    
    public string Name => OfficialName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Extreme;
    public IStrategy? Strategy { get; set; }
    public LinkGraph<ILinkGraphElement> GetGraph(IStrategyManager view)
    {
        view.GraphManager.Construct();
        return view.GraphManager.LinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<ILinkGraphElement> loop)
    {
        loop.ForEachLink((one, two) => ProcessWeakLink(view, one, two), LinkStrength.Weak);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }

    private void ProcessWeakLink(IStrategyManager view, ILinkGraphElement one, ILinkGraphElement two)
    {
        List<CellPossibility> onePoss = new();
        List<CellPossibility> twoPoss = new();

        foreach (var c1 in one.EachElement())
        {
            onePoss.AddRange(c1.ToPossibilityCoordinates());
        }

        foreach (var c2 in two.EachElement())
        {
            twoPoss.AddRange(c2.ToPossibilityCoordinates());
        }

        if (onePoss.Count == 0 || twoPoss.Count == 0) throw new ArgumentException("Wtf");

        if (onePoss.Count == 1 && twoPoss.Count == 1 && onePoss[0] == twoPoss[0])
        {
            var cell = onePoss[0];
            foreach (var possibility in view.PossibilitiesAt(cell.Row, cell.Col))
            {
                if (possibility != cell.Possibility && possibility != twoPoss[0].Possibility)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(possibility, cell.Row, cell.Col);
                }
            }
        }
        else
        {
            foreach (var commons in AllCommons(onePoss, twoPoss))
            {
                foreach (var cell in Cells.SharedSeenCells(commons))
                {
                    view.ChangeBuffer.AddPossibilityToRemove(commons[0].Possibility, cell.Row, cell.Col);
                }
            }
        }
    }

    private List<List<CellPossibility>> AllCommons(List<CellPossibility> onePoss, List<CellPossibility> twoPoss)
    {
        var result = new List<List<CellPossibility>>();
        IPossibilities done = IPossibilities.NewEmpty();
        for (int i = 0; i < onePoss.Count; i++)
        {
            if (done.Peek(onePoss[i].Possibility)) continue;
            
            List<CellPossibility> current = new() { onePoss[i] };
            for (int j = i + 1; j < onePoss.Count; j++)
            {
                if (onePoss[j].Possibility == onePoss[i].Possibility) current.Add(onePoss[j]);
            }

            bool yes = false;
            foreach (var cell in twoPoss)
            {
                if (cell.Possibility == onePoss[i].Possibility)
                {
                    current.Add(cell);
                    yes = true;
                }
            }

            if (yes) result.Add(current);
            done.Add(onePoss[i].Possibility);
        }

        return result;
    }

    public bool ProcessWeakInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility pos) return false;
        view.ChangeBuffer.AddPossibilityToRemove(pos.Possibility, pos.Row, pos.Col);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }

    public bool ProcessStrongInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility pos) return false;
        view.ChangeBuffer.AddSolutionToAdd(pos.Possibility, pos.Row, pos.Col);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }
}