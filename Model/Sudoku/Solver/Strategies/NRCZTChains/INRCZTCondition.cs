using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Sudoku.Solver.Strategies.NRCZTChains;

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



