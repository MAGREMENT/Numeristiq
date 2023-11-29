using System.Linq;
using Global.Enums;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.AlternatingInference.Types;

public class AIType : IAlternatingInferenceType<CellPossibility>
{
    public const string OfficialLoopName = "Alternating Inference Loops";
    public const string OfficialChainName = "Alternating Inference Chains";

    public string LoopName => OfficialLoopName;
    public string ChainName => OfficialChainName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Extreme;
    public IStrategy? Strategy { get; set; }

    public LinkGraph<CellPossibility> GetGraph(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.CellStrongLink, ConstructRule.CellWeakLink,
            ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        return strategyManager.GraphManager.SimpleLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager strategyManager, LinkGraphLoop<CellPossibility> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(strategyManager, one, two), LinkStrength.Weak);
        
        return strategyManager.ChangeBuffer.Commit(Strategy!,
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.NiceLoop));
    }

    private void ProcessWeakLink(IStrategyManager view, CellPossibility one, CellPossibility two)
    {
        if (one.Row == two.Row && one.Col == two.Col)
        {
            RemoveAllExcept(view, one.Row, one.Col, one.Possibility, two.Possibility);
        }
        else
        {
            foreach (var coord in one.SharedSeenCells(two))
            {
                view.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, coord.Row, coord.Col);
            }   
        }
    }
    
    private void RemoveAllExcept(IStrategyManager strategyManager, int row, int col, params int[] except)
    {
        foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
        {
            if (!except.Contains(possibility))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
            }
        }
    }

    public bool ProcessWeakInferenceLoop(IStrategyManager strategyManager, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyManager.ChangeBuffer.ProposePossibilityRemoval(inference.Possibility, inference.Row, inference.Col);
        return strategyManager.ChangeBuffer.Commit(Strategy!,
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInferenceLoop(IStrategyManager strategyManager, CellPossibility inference, LinkGraphLoop<CellPossibility> loop)
    {
        strategyManager.ChangeBuffer.ProposeSolutionAddition(inference.Possibility, inference.Row, inference.Col);
        return strategyManager.ChangeBuffer.Commit(Strategy!,
            new AlternatingInferenceLoopReportBuilder<CellPossibility>(loop, LoopType.StrongInference));
    }

    public bool ProcessChain(IStrategyManager strategyManager, LinkGraphChain<CellPossibility> chain, LinkGraph<CellPossibility> graph)
    {
        return IAlternatingInferenceType<CellPossibility>.ProcessChainWithSimpleGraph(strategyManager,
            chain, graph, Strategy!);
    }
}