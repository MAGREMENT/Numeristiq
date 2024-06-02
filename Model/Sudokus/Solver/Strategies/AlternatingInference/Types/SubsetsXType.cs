using System.Collections.Generic;
using Model.Core;
using Model.Helpers;
using Model.Helpers.Graphs;
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
    public StepDifficulty Difficulty => StepDifficulty.Hard;
    public SudokuStrategy? Strategy { get; set; }
    
    public ILinkGraph<ISudokuElement> GetGraph(ISudokuStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructComplex(SudokuConstructRuleBank.UnitStrongLink, SudokuConstructRuleBank.UnitWeakLink,
            SudokuConstructRuleBank.PointingPossibilities);
        return strategyUser.PreComputer.Graphs.ComplexLinkGraph;
    }

    public bool ProcessFullLoop(ISudokuStrategyUser strategyUser, LinkGraphLoop<ISudokuElement> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyUser, one, two), LinkStrength.Weak);

        return strategyUser.ChangeBuffer.Commit( new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.NiceLoop));
    }
    
    private void ProcessWeakLink(ISudokuStrategyUser view, ISudokuElement one, ISudokuElement two)
    {
        List<Cell> cells = new List<Cell>(one.EnumerateCell());
        cells.AddRange(two.EnumerateCell());

        var possibility = one.EveryPossibilities().FirstPossibility();
        foreach (var cell in SudokuCellUtility.SharedSeenCells(cells))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
        }
    }

    public bool ProcessWeakInferenceLoop(ISudokuStrategyUser strategyUser, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyUser.ChangeBuffer.ProposePossibilityRemoval(single.Possibility, single.Row, single.Column);

        return strategyUser.ChangeBuffer.Commit( new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(ISudokuStrategyUser strategyUser, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyUser.ChangeBuffer.ProposeSolutionAddition(single.Possibility, single.Row, single.Column);
        
        return strategyUser.ChangeBuffer.Commit( new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(ISudokuStrategyUser strategyUser, LinkGraphChain<ISudokuElement> chain, ILinkGraph<ISudokuElement> graph)
    {
        return IAlternatingInferenceType<ISudokuElement>.ProcessChainWithComplexGraph(strategyUser,
            chain, graph, Strategy!);
    }
}