using System.Collections.Generic;
using Model.Sudokus.Solver.Utility;

namespace Model.Sudokus.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopType
{
    public string Name { get; }
    public IEnumerable<CellPossibility[]> Candidates(IStrategyUser strategyUser);
}