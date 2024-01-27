using System.Collections.Generic;
using Model.SudokuSolving.Solver.StrategiesUtility;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;
using Model.SudokuSolving.Solver.StrategiesUtility.NRCZTChains;

namespace Model.SudokuSolving.Solver.Strategies.NRCZTChains;

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



