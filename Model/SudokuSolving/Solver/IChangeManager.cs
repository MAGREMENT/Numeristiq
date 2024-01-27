using Model.SudokuSolving.Solver.Helpers.Changes;
using Model.SudokuSolving.Solver.Helpers.Logs;

namespace Model.SudokuSolving.Solver;

public interface IChangeManager : IPossibilitiesHolder
{
    public IPossibilitiesHolder TakeSnapshot();
    public bool ExecuteChange(SolverChange change);
    public LogManager LogManager { get; }
}