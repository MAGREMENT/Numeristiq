using System.Collections.Generic;

namespace Model.Solver.Strategies.SetEquivalence.Searchers;

public class ExhaustiveSearcher : ISetEquivalenceSearcher
{
    private readonly int _maxOrderDifference;
    private readonly int _maxHouseCount;

    public ExhaustiveSearcher(int maxOrderDifference, int maxHouseCount)
    {
        _maxOrderDifference = maxOrderDifference;
        _maxHouseCount = maxHouseCount;
    }
    
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager)
    {
        yield break;
    }

    
}

