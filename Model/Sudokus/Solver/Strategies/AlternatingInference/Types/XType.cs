using Model.Core;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class XType : IAlternatingInferenceType<CellPossibility>
{
    public const string OfficialLoopName = "X-Cycles";
    public const string OfficialChainName = "X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public Difficulty Difficulty => Difficulty.Hard;
    public SudokuStrategy? Strategy { get; set; }
    public ILinkGraph<CellPossibility> GetGraph(ISudokuSolverData solverData)
    {
        solverData.PreComputer.SimpleGraph.Construct(UnitStrongLinkConstructRule.Instance,
            UnitWeakLinkConstructRule.Instance);
        return solverData.PreComputer.SimpleGraph.Graph;
    }

    public bool ProcessFullLoop(ISudokuSolverData solverData, Loop<CellPossibility, LinkStrength> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(solverData, one, two), LinkStrength.Weak);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.NiceLoop));
        return Strategy!.StopOnFirstCommit;
    }

    private void ProcessWeakLink(ISudokuSolverData view, CellPossibility one, CellPossibility two)
    {
        foreach (var coord in one.SharedSeenCells(two))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, coord.Row, coord.Column);
        }
    }

    public bool ProcessWeakInferenceLoop(ISudokuSolverData solverData, CellPossibility inference, Loop<CellPossibility, LinkStrength> loop)
    {
        solverData.ChangeBuffer.ProposePossibilityRemoval(inference.Possibility, inference.Row, inference.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.WeakInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessStrongInferenceLoop(ISudokuSolverData solverData, CellPossibility inference, Loop<CellPossibility, LinkStrength> loop)
    {
        solverData.ChangeBuffer.ProposeSolutionAddition(inference.Possibility, inference.Row, inference.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.StrongInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessChain(ISudokuSolverData solverData, Chain<CellPossibility, LinkStrength> chain, ILinkGraph<CellPossibility> graph)
    {
        return IAlternatingInferenceType<CellPossibility>.ProcessChainWithSimpleGraph(solverData,
            chain, graph, Strategy!);
    }
}