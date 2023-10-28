using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class SimpleXCycles : IAlternatingChainType<CellPossibility>
{
    public const string OfficialName = "X-Cycles";
    
    public string Name => OfficialName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public IStrategy? Strategy { get; set; }
    public LinkGraph<CellPossibility> GetGraph(IStrategyManager view)
    {
        view.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
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
        foreach (var coord in one.SharedSeenCells(two))
        {
            view.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, coord.Row, coord.Col);
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