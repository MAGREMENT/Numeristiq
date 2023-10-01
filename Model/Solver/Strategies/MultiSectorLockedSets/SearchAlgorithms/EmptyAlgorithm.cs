using System.Collections.Generic;

namespace Model.Solver.Strategies.MultiSectorLockedSets.SearchAlgorithms;

public class EmptyAlgorithm : ICoverHouseSearchAlgorithm
{
    public IEnumerable<SearchResult> Search(IStrategyManager strategyManager)
    {
        yield break;
    }
}