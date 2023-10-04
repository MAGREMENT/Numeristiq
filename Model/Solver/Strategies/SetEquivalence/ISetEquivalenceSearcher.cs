using System.Collections.Generic;

namespace Model.Solver.Strategies.SetEquivalence;

public interface ISetEquivalenceSearcher 
{
    public List<SetEquivalence> Search(IStrategyManager strategyManager);
}