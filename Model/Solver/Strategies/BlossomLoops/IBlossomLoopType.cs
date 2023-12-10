using System.Collections.Generic;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies.BlossomLoops;

public interface IBlossomLoopType
{
    public string Name { get; }
    public IEnumerable<CellPossibility[]> Candidates(IStrategyManager strategyManager);
}