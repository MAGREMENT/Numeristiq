using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.Helpers.Logs;

namespace Model.Sudoku.Solver;

public interface IChangeManager : IPossibilitiesHolder
{
    public IPossibilitiesHolder TakeSnapshot();
    public bool ExecuteChange(SolverChange change);
    public LogManager LogManager { get; }
}