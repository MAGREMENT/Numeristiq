using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies.AlternatingInference.Types;

public class XType : IAlternatingInferenceType<CellPossibility>
{
    public const string OfficialLoopName = "X-Cycles";
    public const string OfficialChainName = "X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public IStrategy? Strategy { get; set; }
    public ILinkGraph<CellPossibility> GetGraph(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        return strategyManager.GraphManager.SimpleLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager strategyManager, LinkGraphLoop<CellPossibility> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyManager, one, two), LinkStrength.Weak);

        return strategyManager.ChangeBuffer.Commit(Strategy!,
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.NiceLoop));
    }

    private void ProcessWeakLink(IStrategyManager view, CellPossibility one, CellPossibility two)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, coord.Row, coord.Column);
        }
    }

    public bool ProcessWeakInferenceLoop(IStrategyManager strategyManager, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyManager.ChangeBuffer.ProposePossibilityRemoval(inference.Possibility, inference.Row, inference.Column);
        return strategyManager.ChangeBuffer.Commit(Strategy!,
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(IStrategyManager strategyManager, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyManager.ChangeBuffer.ProposeSolutionAddition(inference.Possibility, inference.Row, inference.Column);
        return strategyManager.ChangeBuffer.Commit(Strategy!,
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(IStrategyManager strategyManager, LinkGraphChain<CellPossibility> chain, ILinkGraph<CellPossibility> graph)
    {
        return IAlternatingInferenceType<CellPossibility>.ProcessChainWithSimpleGraph(strategyManager,
            chain, graph, Strategy!);
    }
}