using System.Collections.Generic;

namespace Model.SudokuSolving.Solver.Strategies.MultiSector.Searchers;

public class VoidSearcher : ISetEquivalenceSearcher
{
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager)
    {
        yield break;
    }
}