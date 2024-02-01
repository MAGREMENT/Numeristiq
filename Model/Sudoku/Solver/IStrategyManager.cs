using Model.Sudoku.Solver.Helpers;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.StrategiesUtility.AlmostLockedSets;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Model.Sudoku.Solver;

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





