using Global;
using Model.SudokuSolving.Solver.Helpers;
using Model.SudokuSolving.Solver.Helpers.Changes;
using Model.SudokuSolving.Solver.Possibility;
using Model.SudokuSolving.Solver.StrategiesUtility.AlmostLockedSets;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver;

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





