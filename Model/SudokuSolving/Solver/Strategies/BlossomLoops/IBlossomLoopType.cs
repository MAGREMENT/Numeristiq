using System.Collections.Generic;
using Model.SudokuSolving.Solver.StrategiesUtility;

namespace Model.SudokuSolving.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopType
{
    public string Name { get; }
    public IEnumerable<CellPossibility[]> Candidates(IStrategyManager strategyManager);
}