using System.Collections.Generic;

namespace Model.Sudoku.Solver.Strategies.MultiSector;

public interface ISetEquivalenceSearcher 
{
    public IEnumerable<SetEquivalence> Search(IStrategyUser strategyUser);
}