﻿using System.Collections.Generic;
using Model.Core;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class SubsetsXType : IAlternatingInferenceType<ISudokuElement>
{
    public const string OfficialLoopName = "Subsets X-Cycles";
    public const string OfficialChainName = "Subsets X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public Difficulty Difficulty => Difficulty.Hard;
    public SudokuStrategy? Strategy { get; set; }
    
    public ILinkGraph<ISudokuElement> GetGraph(ISudokuSolverData solverData)
    {
        solverData.PreComputer.Graphs.ConstructComplex(SudokuConstructRuleBank.UnitStrongLink, SudokuConstructRuleBank.UnitWeakLink,
            SudokuConstructRuleBank.PointingPossibilities);
        return solverData.PreComputer.Graphs.ComplexLinkGraph;
    }

    public bool ProcessFullLoop(ISudokuSolverData solverData, Loop<ISudokuElement, LinkStrength> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(solverData, one, two), LinkStrength.Weak);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.NiceLoop));
        return Strategy!.StopOnFirstCommit;
    }
    
    private void ProcessWeakLink(ISudokuSolverData view, ISudokuElement one, ISudokuElement two)
    {
        List<Cell> cells = new List<Cell>(one.EnumerateCell());
        cells.AddRange(two.EnumerateCell());

        var possibility = one.EveryPossibilities().FirstPossibility();
        foreach (var cell in SudokuCellUtility.SharedSeenCells(cells))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
        }
    }

    public bool ProcessWeakInferenceLoop(ISudokuSolverData solverData, ISudokuElement inference, Loop<ISudokuElement, LinkStrength> loop)
    {
        if (inference is not CellPossibility single) return false;
        
        solverData.ChangeBuffer.ProposePossibilityRemoval(single.Possibility, single.Row, single.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.WeakInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessStrongInferenceLoop(ISudokuSolverData solverData, ISudokuElement inference, Loop<ISudokuElement, LinkStrength> loop)
    {
        if (inference is not CellPossibility single) return false;
        
        solverData.ChangeBuffer.ProposeSolutionAddition(single.Possibility, single.Row, single.Column);
        if (!solverData.ChangeBuffer.NeedCommit()) return false;

        solverData.ChangeBuffer.Commit(new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.StrongInference));
        return Strategy!.StopOnFirstCommit;
    }

    public bool ProcessChain(ISudokuSolverData solverData, Chain<ISudokuElement, LinkStrength> chain, ILinkGraph<ISudokuElement> graph)
    {
        return IAlternatingInferenceType<ISudokuElement>.ProcessChainWithComplexGraph(solverData,
            chain, graph, Strategy!);
    }
}