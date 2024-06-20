using System.Collections.Generic;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopType
{
    public string Name { get; }
    public IEnumerable<CellPossibility[]> Candidates(ISudokuSolverData solverData);
}