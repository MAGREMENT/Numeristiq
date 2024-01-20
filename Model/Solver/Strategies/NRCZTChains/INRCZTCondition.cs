using System.Collections.Generic;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;
using Model.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Solver.Strategies.NRCZTChains;

public interface INRCZTCondition
{
    public string Name { get; }
    
    public IEnumerable<(CellPossibility, INRCZTConditionChainManipulation)> SearchEndUnderCondition(
        IStrategyManager strategyManager, ILinkGraph<CellPossibility> graph, BlockChain chain, CellPossibility bStart);
}

public interface INRCZTConditionChainManipulation
{
    public void BeforeSearch(BlockChain chain, ILinkGraph<CellPossibility> graph);
    public void AfterSearch(BlockChain chain, ILinkGraph<CellPossibility> graph);
}



