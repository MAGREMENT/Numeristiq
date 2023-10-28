using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Logs;

namespace Model.Solver;

public interface IChangeManager : IPossibilitiesHolder
{
    public IPossibilitiesHolder TakeSnapshot();
    public bool ExecuteChange(SolverChange change);
    public LogManager LogManager { get; }
}