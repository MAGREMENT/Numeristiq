using System.Collections.Generic;

namespace Model.Sudokus.Solver.Strategies.MultiSector;

public interface ISetEquivalenceSearcher 
{
    public IEnumerable<SetEquivalence> Search(ISudokuSolverData solverData);
}