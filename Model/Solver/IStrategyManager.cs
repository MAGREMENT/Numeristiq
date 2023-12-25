using System.Collections.Generic;
using Global;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility.AlmostLockedSets;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver;

public interface IStrategyManager : IPossibilitiesHolder
{ 
    SolverState StartState { get; }
    
    bool LogsManaged { get; }

    ChangeBuffer ChangeBuffer { get; }
    
    LinkGraphManager GraphManager { get; }
    
    PreComputer PreComputer { get; }
    
    AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    
    AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }

    bool UniquenessDependantStrategiesAllowed { get; }

    public Possibilities NotCachedPossibilitiesAt(int row, int col);
    public Possibilities NotCachedPossibilitiesAt(Cell cell)
    {
        return NotCachedPossibilitiesAt(cell.Row, cell.Column);
    }
}





