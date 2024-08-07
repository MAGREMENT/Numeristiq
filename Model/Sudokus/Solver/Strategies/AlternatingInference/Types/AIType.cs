using System.Linq;
using Model.Core;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class AIType : IAlternatingInferenceType<CellPossibility>
{
    public const string OfficialLoopName = "Alternating Inference Loops";
    public const string OfficialChainName = "Alternating Inference Chains";

    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public Difficulty Difficulty => Difficulty.Extreme;
    public SudokuStrategy? Strategy { get; set; }

    public ILinkGraph<CellPossibility> GetGraph(ISudokuSolverData solverData)
    {
        solverData.PreComputer.Graphs.ConstructSimple(SudokuConstructRuleBank.CellStrongLink,SudokuConstructRuleBank.CellWeakLink,
            SudokuConstructRuleBank.UnitStrongLink, SudokuConstructRuleBank.UnitWeakLink);
        return solverData.PreComputer.Graphs.SimpleLinkGraph;
    }

    public bool ProcessFullLoop(ISudokuSolverData solverData, LinkGraphLoop<CellPossibility> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(solverData, one, two), LinkStrength.Weak);
        
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.NiceLoop));
        return Strategy!.StopOnFirstCommit;
    }

    private void ProcessWeakLink(ISudokuSolverData view, CellPossibility one, CellPossibility two)
    {
        if (one.Row == two.Row && one.Column == two.Column)
        {
            RemoveAllExcept(view, one.Row, one.Column, one.Possibility, two.Possibility);
        }
        else
        {
            foreach (var coord in one.SharedSeenCells(two))
            {
                view.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, coord.Row, coord.Column);
            }   
        }
    }
    
    private void RemoveAllExcept(ISudokuSolverData solverData, int row, int col, params int[] except)
    {
        foreach (var possibility in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
        {
            if (!except.Contains(possibility))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }
    }

    public bool ProcessWeakInferenceLoop(ISudokuSolverData solverData, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        solverData.ChangeBuffer.ProposePossibilityRemoval(inference.Possibility, inference.Row, inference.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.WeakInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessStrongInferenceLoop(ISudokuSolverData solverData, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        solverData.ChangeBuffer.ProposeSolutionAddition(inference.Possibility, inference.Row, inference.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.StrongInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessChain(ISudokuSolverData solverData, LinkGraphChain<CellPossibility> chain, ILinkGraph<CellPossibility> graph)
    {
        return IAlternatingInferenceType<CellPossibility>.ProcessChainWithSimpleGraph(solverData,
            chain, graph, Strategy!);
    }
}