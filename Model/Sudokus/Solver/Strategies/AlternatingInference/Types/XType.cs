using Model.Core;
using Model.Helpers;
using Model.Helpers.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class XType : IAlternatingInferenceType<CellPossibility>
{
    public const string OfficialLoopName = "X-Cycles";
    public const string OfficialChainName = "X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StepDifficulty Difficulty => StepDifficulty.Hard;
    public SudokuStrategy? Strategy { get; set; }
    public ILinkGraph<CellPossibility> GetGraph(ISudokuStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(SudokuConstructRuleBank.UnitStrongLink, SudokuConstructRuleBank.UnitWeakLink);
        return strategyUser.PreComputer.Graphs.SimpleLinkGraph;
    }

    public bool ProcessFullLoop(ISudokuStrategyUser strategyUser, LinkGraphLoop<CellPossibility> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyUser, one, two), LinkStrength.Weak);

        return strategyUser.ChangeBuffer.Commit(
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.NiceLoop));
    }

    private void ProcessWeakLink(ISudokuStrategyUser view, CellPossibility one, CellPossibility two)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, coord.Row, coord.Column);
        }
    }

    public bool ProcessWeakInferenceLoop(ISudokuStrategyUser strategyUser, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyUser.ChangeBuffer.ProposePossibilityRemoval(inference.Possibility, inference.Row, inference.Column);
        return strategyUser.ChangeBuffer.Commit(
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(ISudokuStrategyUser strategyUser, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyUser.ChangeBuffer.ProposeSolutionAddition(inference.Possibility, inference.Row, inference.Column);
        return strategyUser.ChangeBuffer.Commit(
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(ISudokuStrategyUser strategyUser, LinkGraphChain<CellPossibility> chain, ILinkGraph<CellPossibility> graph)
    {
        return IAlternatingInferenceType<CellPossibility>.ProcessChainWithSimpleGraph(strategyUser,
            chain, graph, Strategy!);
    }
}