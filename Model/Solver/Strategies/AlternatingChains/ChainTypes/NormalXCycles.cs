﻿using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class NormalXCycles : IAlternatingChainType<CellPossibility>
{
    public string Name => "XCycles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public IStrategy? Strategy { get; set; }
    public LinkGraph<CellPossibility> GetGraph(IStrategyManager view)
    {
        return new LinkGraph<CellPossibility>(); //TODO
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<CellPossibility> loop)
    {
        bool wasProgressMade = false;
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two, out wasProgressMade), LinkStrength.Weak);

        return wasProgressMade;
    }

    private void ProcessWeakLink(IStrategyManager view, CellPossibility one, CellPossibility two, out bool wasProgressMade)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            if (view.RemovePossibility(one.Possibility, coord.Row, coord.Col, Strategy!)) wasProgressMade = true;
        }

        wasProgressMade = false;
    }

    public bool ProcessWeakInference(IStrategyManager view, CellPossibility inference, Loop<CellPossibility> loop)
    {
        return view.RemovePossibility(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }

    public bool ProcessStrongInference(IStrategyManager view, CellPossibility inference, Loop<CellPossibility> loop)
    {
        return view.AddSolution(inference.Possibility, inference.Row, inference.Col, Strategy!);
    }
}