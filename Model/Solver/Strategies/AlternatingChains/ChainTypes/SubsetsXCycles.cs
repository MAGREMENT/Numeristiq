using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class SubsetsXCycles : IAlternatingChainType<ILinkGraphElement>
{
    public const string OfficialName = "Subsets X-Cycles";
    
    public string Name => OfficialName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public IStrategy? Strategy { get; set; }
    
    public LinkGraph<ILinkGraphElement> GetGraph(IStrategyManager view)
    {
        view.GraphManager.ConstructComplex(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink,
            ConstructRule.PointingPossibilities);
        return view.GraphManager.ComplexLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<ILinkGraphElement> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two), LinkStrength.Weak);

        return view.ChangeBuffer.Commit(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop, LoopType.NiceLoop));
    }
    
    private void ProcessWeakLink(IStrategyManager view, ILinkGraphElement one, ILinkGraphElement two)
    {
        List<Cell> cells = new List<Cell>(one.EveryCell());
        cells.AddRange(two.EveryCell());

        var possibility = one.EveryPossibilities().First();
        foreach (var cell in Cells.SharedSeenCells(cells))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
        }
    }

    public bool ProcessWeakInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        view.ChangeBuffer.ProposePossibilityRemoval(single.Possibility, single.Row, single.Col);

        return view.ChangeBuffer.Commit(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        view.ChangeBuffer.ProposeSolutionAddition(single.Possibility, single.Row, single.Col);
        
        return view.ChangeBuffer.Commit(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop, LoopType.StrongInference));
    }
}