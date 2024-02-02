using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies.AlternatingInference.Types;

public class SubsetsXType : IAlternatingInferenceType<ISudokuElement>
{
    public const string OfficialLoopName = "Subsets X-Cycles";
    public const string OfficialChainName = "Subsets X-Chains";
    
    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public IStrategy? Strategy { get; set; }
    
    public ILinkGraph<ISudokuElement> GetGraph(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructComplex(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink,
            ConstructRule.PointingPossibilities);
        return strategyUser.PreComputer.Graphs.ComplexLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyUser strategyUser, LinkGraphLoop<ISudokuElement> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyUser, one, two), LinkStrength.Weak);

        return strategyUser.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.NiceLoop));
    }
    
    private void ProcessWeakLink(IStrategyUser view, ISudokuElement one, ISudokuElement two)
    {
        List<Cell> cells = new List<Cell>(one.EveryCell());
        cells.AddRange(two.EveryCell());

        var possibility = one.EveryPossibilities().First();
        foreach (var cell in Cells.SharedSeenCells(cells))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
        }
    }

    public bool ProcessWeakInferenceLoop(IStrategyUser strategyUser, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyUser.ChangeBuffer.ProposePossibilityRemoval(single.Possibility, single.Row, single.Column);

        return strategyUser.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(IStrategyUser strategyUser, ISudokuElement inference, LinkGraphLoop<ISudokuElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        strategyUser.ChangeBuffer.ProposeSolutionAddition(single.Possibility, single.Row, single.Column);
        
        return strategyUser.ChangeBuffer.Commit(Strategy!, new AlternatingInferenceLoopReportBuilder<ISudokuElement>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(IStrategyUser strategyUser, LinkGraphChain<ISudokuElement> chain, ILinkGraph<ISudokuElement> graph)
    {
        return IAlternatingInferenceType<ISudokuElement>.ProcessChainWithComplexGraph(strategyUser,
            chain, graph, Strategy!);
    }
}