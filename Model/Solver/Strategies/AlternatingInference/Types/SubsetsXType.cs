using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.AlternatingInference.Types;

public class SubsetsXType : IAlternatingInferenceType<ILinkGraphElement>
{
    public const string OfficialLoopName = "Subsets X-Cycles";
    public const string OfficialChainName = "Subsets X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public IStrategy? Strategy { get; set; }
    
    public LinkGraph<ILinkGraphElement> GetGraph(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructComplex(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink,
            ConstructRule.PointingPossibilities);
        return strategyManager.GraphManager.ComplexLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager strategyManager, LinkGraphLoop<ILinkGraphElement> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyManager, one, two), LinkStrength.Weak);

        return strategyManager.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ILinkGraphElement>(loop, LoopType.NiceLoop));
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

    public bool ProcessWeakInferenceLoop(IStrategyManager strategyManager, ILinkGraphElement inference, LinkGraphLoop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyManager.ChangeBuffer.ProposePossibilityRemoval(single.Possibility, single.Row, single.Column);

        return strategyManager.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ILinkGraphElement>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(IStrategyManager strategyManager, ILinkGraphElement inference, LinkGraphLoop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyManager.ChangeBuffer.ProposeSolutionAddition(single.Possibility, single.Row, single.Column);
        
        return strategyManager.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ILinkGraphElement>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(IStrategyManager strategyManager, LinkGraphChain<ILinkGraphElement> chain, LinkGraph<ILinkGraphElement> graph)
    {
        return IAlternatingInferenceType<ILinkGraphElement>.ProcessChainWithComplexGraph(strategyManager,
            chain, graph, Strategy!);
    }
}