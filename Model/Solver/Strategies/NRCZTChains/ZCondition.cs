using System.Collections.Generic;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;
using Model.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Solver.Strategies.NRCZTChains;

public class ZCondition : INRCZTCondition
{
    public string Name => "Z";

    public IEnumerable<(CellPossibility, INRCZTConditionChainManipulation)> SearchEndUnderCondition(IStrategyManager strategyManager, LinkGraph<CellPossibility> graph, BlockChain chain,
        CellPossibility bStart)
    {
        yield break;
    }
}