using Model.Sudokus.Solver.StrategiesUtility;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class XType : IAlternatingInferenceType<CellPossibility>
{
    public const string OfficialLoopName = "X-Cycles";
    public const string OfficialChainName = "X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public SudokuStrategy? Strategy { get; set; }
    public ILinkGraph<CellPossibility> GetGraph(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        return strategyUser.PreComputer.Graphs.SimpleLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyUser strategyUser, LinkGraphLoop<CellPossibility> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyUser, one, two), LinkStrength.Weak);

        return strategyUser.ChangeBuffer.Commit(
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.NiceLoop));
    }

    private void ProcessWeakLink(IStrategyUser view, CellPossibility one, CellPossibility two)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, coord.Row, coord.Column);
        }
    }

    public bool ProcessWeakInferenceLoop(IStrategyUser strategyUser, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyUser.ChangeBuffer.ProposePossibilityRemoval(inference.Possibility, inference.Row, inference.Column);
        return strategyUser.ChangeBuffer.Commit(
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(IStrategyUser strategyUser, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyUser.ChangeBuffer.ProposeSolutionAddition(inference.Possibility, inference.Row, inference.Column);
        return strategyUser.ChangeBuffer.Commit(
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(IStrategyUser strategyUser, LinkGraphChain<CellPossibility> chain, ILinkGraph<CellPossibility> graph)
    {
        return IAlternatingInferenceType<CellPossibility>.ProcessChainWithSimpleGraph(strategyUser,
            chain, graph, Strategy!);
    }
}