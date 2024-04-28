using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class SubsetsAIType : IAlternatingInferenceType<ISudokuElement>
{
    public const string OfficialLoopName = "Subsets Alternating Inference Loops";
    public const string OfficialChainName = "Subsets Alternating Inference Chains";

    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Extreme;
    public SudokuStrategy? Strategy { get; set; }
    public ILinkGraph<ISudokuElement> GetGraph(ISudokuStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructComplex(SudokuConstructRuleBank.CellStrongLink, SudokuConstructRuleBank.CellWeakLink,
            SudokuConstructRuleBank.UnitStrongLink, SudokuConstructRuleBank.UnitWeakLink, SudokuConstructRuleBank.PointingPossibilities,
            SudokuConstructRuleBank.AlmostNakedPossibilities, SudokuConstructRuleBank.JuniorExocet);
        return strategyUser.PreComputer.Graphs.ComplexLinkGraph;
    }

    public bool ProcessFullLoop(ISudokuStrategyUser strategyUser, LinkGraphLoop<ISudokuElement> loop)
    {
        loop.ForEachLink((one, two) => ProcessWeakLink(strategyUser, one, two), LinkStrength.Weak);

        return strategyUser.ChangeBuffer.Commit( new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.NiceLoop));
    }

    private void ProcessWeakLink(ISudokuStrategyUser view, ISudokuElement one, ISudokuElement two)
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

            foreach (var cell in SudokuCellUtility.SharedSeenCells(cells))
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
                
                foreach (var cell in SudokuCellUtility.SharedSeenCells(cells))
                {
                    view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell.Row, cell.Column);
                }
            }
        }
    }

    public bool ProcessWeakInferenceLoop(ISudokuStrategyUser strategyUser, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility pos) return false;
        strategyUser.ChangeBuffer.ProposePossibilityRemoval(pos.Possibility, pos.Row, pos.Column);

        return strategyUser.ChangeBuffer.Commit( new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(ISudokuStrategyUser strategyUser, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility pos) return false;
        strategyUser.ChangeBuffer.ProposeSolutionAddition(pos.Possibility, pos.Row, pos.Column);

        return strategyUser.ChangeBuffer.Commit( new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(ISudokuStrategyUser strategyUser, LinkGraphChain<ISudokuElement> chain, ILinkGraph<ISudokuElement> graph)
    {
        return IAlternatingInferenceType<ISudokuElement>.ProcessChainWithComplexGraph(strategyUser,
            chain, graph, Strategy!);
    }
}