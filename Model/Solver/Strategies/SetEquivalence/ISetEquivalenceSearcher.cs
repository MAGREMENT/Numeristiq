using System.Collections.Generic;

namespace Model.Solver.Strategies.SetEquivalence;

public interface ISetEquivalenceSearcher 
{
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager);
}