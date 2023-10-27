using System.Collections.Generic;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class ComplexXCycles : IAlternatingChainType<ILinkGraphElement>
{
    public const string OfficialName = "X-Cycles";
    
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
        
        RemovePossibilityInAll(view, Cells.SharedSeenCells(cells), one.EveryPossibilities().First());
    }
    
    private void RemovePossibilityInAll(IStrategyManager view, IEnumerable<Cell> coords, int possibility)
    {
        foreach (var coord in coords)
        {
            view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
        }
    }

    public bool ProcessWeakInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        view.ChangeBuffer.AddPossibilityToRemove(single.Possibility, single.Row, single.Col);

        return view.ChangeBuffer.Commit(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        view.ChangeBuffer.AddSolutionToAdd(single.Possibility, single.Row, single.Col);
        
        return view.ChangeBuffer.Commit(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop, LoopType.StrongInference));
    }
}