using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.AlternatingInference.Types;

public class SubsetsXType : IAlternatingInferenceType<ISudokuElement>
{
    public const string OfficialLoopName = "Subsets X-Cycles";
    public const string OfficialChainName = "Subsets X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public IStrategy? Strategy { get; set; }
    
    public ILinkGraph<ISudokuElement> GetGraph(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructComplex(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink,
            ConstructRule.PointingPossibilities);
        return strategyManager.GraphManager.ComplexLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager strategyManager, LinkGraphLoop<ISudokuElement> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyManager, one, two), LinkStrength.Weak);

        return strategyManager.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.NiceLoop));
    }
    
    private void ProcessWeakLink(IStrategyManager view, ISudokuElement one, ISudokuElement two)
    {
        List<Cell> cells = new List<Cell>(one.EveryCell());
        cells.AddRange(two.EveryCell());

        var possibility = one.EveryPossibilities().First();
        foreach (var cell in Cells.SharedSeenCells(cells))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
        }
    }

    public bool ProcessWeakInferenceLoop(IStrategyManager strategyManager, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyManager.ChangeBuffer.ProposePossibilityRemoval(single.Possibility, single.Row, single.Column);

        return strategyManager.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(IStrategyManager strategyManager, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyManager.ChangeBuffer.ProposeSolutionAddition(single.Possibility, single.Row, single.Column);
        
        return strategyManager.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(IStrategyManager strategyManager, LinkGraphChain<ISudokuElement> chain, ILinkGraph<ISudokuElement> graph)
    {
        return IAlternatingInferenceType<ISudokuElement>.ProcessChainWithComplexGraph(strategyManager,
            chain, graph, Strategy!);
    }
}