using System.Collections.Generic;

namespace Model.SudokuSolving.Solver.Strategies.MultiSector;

public interface ISetEquivalenceSearcher 
{
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager);
}