using System.Collections.Generic;
using Model.Core;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class SubsetsAIType : IAlternatingInferenceType<ISudokuElement>
{
    public const string OfficialLoopName = "Subsets Alternating Inference Loops";
    public const string OfficialChainName = "Subsets Alternating Inference Chains";

    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public Difficulty Difficulty => Difficulty.Extreme;
    public SudokuStrategy? Strategy { get; set; }
    public IGraph<ISudokuElement, LinkStrength> GetGraph(ISudokuSolverData solverData)
    {
        solverData.PreComputer.ComplexGraph.Construct(CellStrongLinkConstructRule.Instance, CellWeakLinkConstructRule.Instance,
            UnitStrongLinkConstructRule.Instance, UnitWeakLinkConstructRule.Instance,
            PointingPossibilitiesConstructRule.Instance, AlmostNakedSetConstructRule.Instance);
        return solverData.PreComputer.ComplexGraph.Graph;
    }

    public bool ProcessFullLoop(ISudokuSolverData solverData, Loop<ISudokuElement, LinkStrength> loop)
    {
        loop.ForEachLink((one, two) => ProcessWeakLink(solverData, one, two), LinkStrength.Weak);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.NiceLoop));
        return Strategy!.StopOnFirstCommit;
    }

    private static void ProcessWeakLink(ISudokuSolverData view, ISudokuElement one, ISudokuElement two)
    {
        var cp1 = one.EveryCellPossibilities();
        var pos1 = one.EveryPossibilities();
        var cp2 = two.EveryCellPossibilities();
        var pos2 = two.EveryPossibilities();

        if (cp1.Length == 1 && cp2.Length == 1 && pos1.Count == 1 && pos2.Count == 1 && cp1[0].Cell == cp2[0].Cell)
        {
            foreach (var possibility in view.PossibilitiesAt(cp1[0].Cell).EnumeratePossibilities())
            {
                if (pos1.Contains(possibility) || pos2.Contains(possibility)) continue;
                
                view.ChangeBuffer.ProposePossibilityRemoval(possibility, cp1[0].Cell.Row, cp1[0].Cell.Column);
            }

            return;
        }

        var and = pos1 & pos2;

        foreach (var possibility in and.EnumeratePossibilities())
        {
            List<Cell> cells = new();
            
            foreach (var cp in cp1)
            {
                if (cp.Possibilities.Contains(possibility)) cells.Add(cp.Cell);
            }
            
            foreach (var cp in cp2)
            {
                if (cp.Possibilities.Contains(possibility)) cells.Add(cp.Cell);
            }

            foreach (var cell in SudokuUtility.SharedSeenCells(cells))
            {
                view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
            }
        }

        if (one is NakedSet ans && two is CellPossibility cellPossibility)
        {
            foreach (var possibility in ans.EnumeratePossibilities())
            {
                if (possibility == cellPossibility.Possibility) continue;
                
                List<Cell> cells = new();

                foreach (var cp in ans.EnumerateCellPossibilities())
                {
                    if (cp.Possibilities.Contains(possibility)) cells.Add(cp.Cell);
                }
                
                foreach (var cell in SudokuUtility.SharedSeenCells(cells))
                {
                    view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                }
            }
        }
    }

    public bool ProcessWeakInferenceLoop(ISudokuSolverData solverData, ISudokuElement inference, Loop<ISudokuElement, LinkStrength> loop)
    {
        if (inference is not CellPossibility pos) return false;
        
        solverData.ChangeBuffer.ProposePossibilityRemoval(pos.Possibility, pos.Row, pos.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.WeakInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessStrongInferenceLoop(ISudokuSolverData solverData, ISudokuElement inference, Loop<ISudokuElement, LinkStrength> loop)
    {
        if (inference is not CellPossibility pos) return false;
        
        solverData.ChangeBuffer.ProposeSolutionAddition(pos.Possibility, pos.Row, pos.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.StrongInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessChain(ISudokuSolverData solverData, Chain<ISudokuElement, LinkStrength> chain, IGraph<ISudokuElement, LinkStrength> graph)
    {
        return IAlternatingInferenceType<ISudokuElement>.ProcessChainWithComplexGraph(solverData,
            chain, graph, Strategy!);
    }
}