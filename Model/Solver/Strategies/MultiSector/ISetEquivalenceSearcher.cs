using System.Collections.Generic;

namespace Model.Solver.Strategies.MultiSector;

public interface ISetEquivalenceSearcher 
{
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager);
}