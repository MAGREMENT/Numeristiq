using System.Linq;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class SimpleAlternatingInferenceChains : IAlternatingChainType<CellPossibility>
{
    public const string OfficialName = "Alternating Inference Chains";
    
    public string Name => OfficialName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Extreme;
    public IStrategy? Strategy { get; set; }

    public LinkGraph<CellPossibility> GetGraph(IStrategyManager view)
    {
        view.GraphManager.ConstructSimple(ConstructRule.CellStrongLink, ConstructRule.CellWeakLink,
            ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        return view.GraphManager.SimpleLinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<CellPossibility> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two), LinkStrength.Weak);
        
        return view.ChangeBuffer.Commit(Strategy!,
            new AlternatingChainReportBuilder<CellPossibility>(loop, LoopType.NiceLoop));
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

    public bool ProcessWeakInference(IStrategyManager view, CellPossibility inference, Loop<CellPossibility> loop)
    {
        view.ChangeBuffer.ProposePossibilityRemoval(inference.Possibility, inference.Row, inference.Col);
        return view.ChangeBuffer.Commit(Strategy!,
            new AlternatingChainReportBuilder<CellPossibility>(loop, LoopType.WeakInference));
    }

    public bool ProcessStrongInference(IStrategyManager view, CellPossibility inference, Loop<CellPossibility> loop)
    {
        view.ChangeBuffer.ProposeSolutionAddition(inference.Possibility, inference.Row, inference.Col);
        return view.ChangeBuffer.Commit(Strategy!,
            new AlternatingChainReportBuilder<CellPossibility>(loop, LoopType.StrongInference));
    }
}