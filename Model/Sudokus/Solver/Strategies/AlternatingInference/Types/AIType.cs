using System.Linq;
using Model.Helpers;
using Model.Helpers.Graphs;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Types;

public class AIType : IAlternatingInferenceType<CellPossibility>
{
    public const string OfficialLoopName = "Alternating Inference Loops";
    public const string OfficialChainName = "Alternating Inference Chains";

    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Extreme;
    public SudokuStrategy? Strategy { get; set; }

    public ILinkGraph<CellPossibility> GetGraph(ISudokuStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(SudokuConstructRuleBank.CellStrongLink,SudokuConstructRuleBank.CellWeakLink,
            SudokuConstructRuleBank.UnitStrongLink, SudokuConstructRuleBank.UnitWeakLink);
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
    
    private void RemoveAllExcept(ISudokuStrategyUser strategyUser, int row, int col, params int[] except)
    {
        foreach (var possibility in strategyUser.PossibilitiesAt(row, col).EnumeratePossibilities())
        {
            if (!except.Contains(possibility))
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
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