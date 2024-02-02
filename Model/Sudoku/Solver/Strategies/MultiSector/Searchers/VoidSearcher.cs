using System.Collections.Generic;

namespace Model.Sudoku.Solver.Strategies.MultiSector.Searchers;

public class VoidSearcher : ISetEquivalenceSearcher
{
    public IEnumerable<SetEquivalence> Search(IStrategyUser strategyUser)
    {
        yield break;
    }
}